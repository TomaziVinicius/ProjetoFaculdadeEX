using Microsoft.EntityFrameworkCore;
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
        var serverVersion = ServerVersion.AutoDetect(connectionString);
        options.UseMySql(connectionString, serverVersion);
    }
    catch
    {
        // Caso o MySQL ainda não esteja iniciado no momento da inicialização
        options.UseMySql(connectionString, new MySqlServerVersion(new Version(8, 0, 36)));
    }
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// 2. Registro dos Repositórios e Serviços de Domínio e Aplicação
builder.Services.AddInfrastructureRepositories();
builder.Services.AddApplicationServices();

var app = builder.Build();

// 2. Auto-criação e Carga Inicial (Seed) do Banco de Dados
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<AppDbContext>();
        context.Database.EnsureCreated();
        DbSeeder.Seed(context);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogWarning(ex, "Aviso: Banco MySQL local não conectado no momento. Inicie o serviço MySQL para aplicar a persistência.");
    }
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
