using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using TriagemImagem.Application.DTOs;
using TriagemImagem.Application.Extensions;
using TriagemImagem.Application.Interfaces;
using TriagemImagem.Domain.Exceptions;
using TriagemImagem.Infrastructure.Data.Context;
using TriagemImagem.Infrastructure.Extensions;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? "Server=localhost;Port=3306;Database=triagem_imagem_db;User=root;Password=;TreatTinyAsBoolean=true;";

builder.Services.AddDbContext<AppDbContext>(options =>
{
    try
    {
        options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString));
    }
    catch
    {
        options.UseMySql(connectionString, new MySqlServerVersion(new Version(8, 0, 36)));
    }
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddCors(options => options.AddPolicy("Frontend", policy => policy
    .AllowAnyOrigin()
    .AllowAnyHeader()
    .AllowAnyMethod()));
builder.Services.AddInfrastructureRepositories();
builder.Services.AddApplicationServices();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    try
    {
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        context.Database.EnsureCreated();
        DbSeeder.Seed(context);
    }
    catch (Exception ex)
    {
        scope.ServiceProvider.GetRequiredService<ILogger<Program>>()
            .LogWarning(ex, "Banco MySQL local não conectado. Inicie o serviço para aplicar a persistência.");
    }
}

app.Use(async (context, next) =>
{
    try
    {
        await next();
    }
    catch (DomainException ex)
    {
        context.Response.StatusCode = StatusCodes.Status400BadRequest;
        await context.Response.WriteAsJsonAsync(new { success = false, message = ex.Message });
    }
    catch (Exception ex)
    {
        app.Logger.LogError(ex, "Erro não tratado em {Method} {Path}", context.Request.Method, context.Request.Path);
        context.Response.StatusCode = StatusCodes.Status500InternalServerError;
        await context.Response.WriteAsJsonAsync(new { success = false, message = "Ocorreu um erro interno no servidor." });
    }
});

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("Frontend");
app.UseAuthorization();
app.MapControllers();

app.MapPost("/api/v1/auth/login", async (LoginRequestDto request, IAuthAppService service) =>
{
    var usuario = await service.AutenticarAsync(request);
    return usuario is null
        ? Results.Json(new { success = false, message = "Login ou senha inválidos." }, statusCode: StatusCodes.Status401Unauthorized)
        : Results.Ok(usuario);
});

app.MapGet("/api/v1/worklist", async ([AsParameters] FiltroWorklistDto filtro, IWorklistAppService service) =>
    Results.Ok(await service.ObterWorklistAsync(filtro)));

app.MapGet("/api/v1/worklist/{exameId:int}", async (int exameId, IWorklistAppService service) =>
{
    var exame = await service.ObterExamePorIdAsync(exameId);
    return exame is null
        ? Results.Json(new { success = false, message = "Exame não encontrado." }, statusCode: StatusCodes.Status404NotFound)
        : Results.Ok(exame);
});

app.MapPost("/api/v1/worklist/{exameId:int}/chamar", async (int exameId, ChamarPacienteRequest request, IWorklistAppService service) =>
{
    var chamado = await service.ChamarPacienteAsync(exameId, request.UsuarioId);
    return chamado
        ? Results.Ok(new { success = true, message = "Paciente chamado com sucesso." })
        : Results.Json(new { success = false, message = "Exame não encontrado." }, statusCode: StatusCodes.Status404NotFound);
});

app.MapPost("/api/v1/triagem", async (SalvarTriagemRequestDto request, ITriagemAppService service) =>
    Results.Ok(await service.SalvarTriagemAsync(request)));

app.MapGet("/api/v1/triagem/exame/{exameId:int}", async (int exameId, ITriagemAppService service) =>
{
    var triagem = await service.ObterPorExameIdAsync(exameId);
    return triagem is null
        ? Results.Json(new { success = false, message = "Triagem não encontrada para este exame." }, statusCode: StatusCodes.Status404NotFound)
        : Results.Ok(triagem);
});

app.MapPost("/api/v1/triagem/calcular-dose", (CalcularDoseRequestDto request, ICalculoDoseAppService service) =>
    Results.Ok(service.CalcularDose(request)));

app.MapPost("/api/v1/triagem/{exameId:int}/liberar", async (int exameId, LiberarExameRequest request, ITriagemAppService service) =>
{
    var liberado = await service.LiberarExameAsync(exameId, request.UsuarioId, request.Observacao);
    return liberado
        ? Results.Ok(new { success = true, message = "Exame liberado para execução." })
        : Results.Json(new { success = false, message = "Exame não encontrado." }, statusCode: StatusCodes.Status404NotFound);
});

app.MapPost("/api/v1/triagem/{exameId:int}/cancelar", async (int exameId, CancelarExameRequest request, ITriagemAppService service) =>
{
    var cancelado = await service.CancelarExameAsync(exameId, request.UsuarioId, request.Motivo);
    return cancelado
        ? Results.Ok(new { success = true, message = "Exame cancelado." })
        : Results.Json(new { success = false, message = "Exame não encontrado." }, statusCode: StatusCodes.Status404NotFound);
});

app.MapGet("/api/v1/auditoria", async (int? limite, IAuditoriaAppService service) =>
    Results.Ok(await service.ObterUltimosLogsAsync(Math.Clamp(limite ?? 50, 1, 200))));

app.Run();

public sealed class ChamarPacienteRequest
{
    public int UsuarioId { get; set; }
}

public sealed class LiberarExameRequest
{
    public int UsuarioId { get; set; }
    public string? Observacao { get; set; }
}

public sealed class CancelarExameRequest
{
    public int UsuarioId { get; set; }
    public string Motivo { get; set; } = string.Empty;
}
