using Microsoft.EntityFrameworkCore;
using TriagemImagem.Api.Middlewares;
using TriagemImagem.Application.Extensions;
using TriagemImagem.Infrastructure.Data.Context;
using TriagemImagem.Infrastructure.Extensions;

var builder = WebApplication.CreateBuilder(args);

// 1. Configuração do Banco de Dados MySQL / MariaDB via Pomelo EF Core
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

// 2. Controladores REST e Documentação Swagger OpenAPI
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "Sistema de Triagem para Diagnóstico por Imagem (TC & RM)",
        Version = "v1",
        Description = "API RESTful desenvolvida sob Onion Architecture para triagem clínica, checklist da aviação e semáforo de risco."
    });
});

// 3. Política de CORS para permitir comunicação com o Frontend
builder.Services.AddCors(options =>
{
    options.AddPolicy("Frontend", policy => policy
        .AllowAnyOrigin()
        .AllowAnyHeader()
        .AllowAnyMethod());
});

// 4. Registro dos Repositórios e Serviços de Domínio e Aplicação
builder.Services.AddInfrastructureRepositories();
builder.Services.AddApplicationServices();

var app = builder.Build();

// 5. Auto-criação e Carga Inicial (Seed) do Banco de Dados
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

// 6. Middleware Global para Tratamento de Erros e Regras Clínicas (DomainException)
app.UseMiddleware<ExceptionHandlingMiddleware>();

// 7. Pipeline HTTP
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("Frontend");
app.UseAuthorization();
app.MapControllers();

app.Run();

// Necessário para referenciar em testes de integração (WebApplicationFactory)
public partial class Program { }
