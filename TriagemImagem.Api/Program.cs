using System.Net.Sockets;
using Microsoft.EntityFrameworkCore;
using TriagemImagem.Api.Middlewares;
using TriagemImagem.Application.Extensions;
using TriagemImagem.Infrastructure.Data.Context;
using TriagemImagem.Infrastructure.Extensions;

var builder = WebApplication.CreateBuilder(args);

// 1. Configuração do Banco de Dados com Detecção Automática (MySQL se ativo na porta, ou SQLite Portátil)
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? "Server=localhost;Port=3306;Database=triagem_imagem_db;User=root;Password=;TreatTinyAsBoolean=true;";

var (mysqlHost, mysqlPort) = ExtrairHostPorta(connectionString);
var mysqlDisponivel = TestarConexaoTcp(mysqlHost, mysqlPort, 400);

builder.Services.AddDbContext<AppDbContext>(options =>
{
    if (mysqlDisponivel)
    {
        try
        {
            options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString));
        }
        catch
        {
            options.UseMySql(connectionString, new MySqlServerVersion(new Version(8, 0, 36)));
        }
    }
    else
    {
        var dbPath = Path.Combine(AppContext.BaseDirectory, "triagem_imagem.db");
        options.UseSqlite($"Data Source={dbPath}");
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
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
        var provedor = context.Database.ProviderName?.Contains("Sqlite") == true ? "SQLite Portátil" : "MySQL";
        logger.LogInformation("Banco de dados pronto para uso ({Provedor}).", provedor);
    }
    catch (Exception ex)
    {
        scope.ServiceProvider.GetRequiredService<ILogger<Program>>()
            .LogWarning(ex, "Aviso ao inicializar persistência do banco.");
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

public partial class Program 
{ 
    public static (string host, int port) ExtrairHostPorta(string connStr)
    {
        var host = "localhost";
        var port = 3306;
        foreach (var part in connStr.Split(';', StringSplitOptions.RemoveEmptyEntries))
        {
            var kv = part.Split('=', 2);
            if (kv.Length == 2)
            {
                var key = kv[0].Trim().ToLowerInvariant();
                var val = kv[1].Trim();
                if (key == "server" || key == "host") host = val;
                if (key == "port" && int.TryParse(val, out var p)) port = p;
            }
        }
        return (host, port);
    }

    public static bool TestarConexaoTcp(string host, int port, int timeoutMs)
    {
        try
        {
            using var client = new TcpClient();
            var ar = client.BeginConnect(host, port, null, null);
            var success = ar.AsyncWaitHandle.WaitOne(TimeSpan.FromMilliseconds(timeoutMs));
            if (success && client.Connected)
            {
                client.EndConnect(ar);
                return true;
            }
            return false;
        }
        catch
        {
            return false;
        }
    }
}
