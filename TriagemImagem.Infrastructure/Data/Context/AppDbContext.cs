using Microsoft.EntityFrameworkCore;
using TriagemImagem.Domain.Entities;

namespace TriagemImagem.Infrastructure.Data.Context;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Usuario> Usuarios => Set<Usuario>();
    public DbSet<Paciente> Pacientes => Set<Paciente>();
    public DbSet<Exame> Exames => Set<Exame>();
    public DbSet<FormularioTriagem> FormulariosTriagem => Set<FormularioTriagem>();
    public DbSet<ItemRespostaTriagem> ItensRespostaTriagem => Set<ItemRespostaTriagem>();
    public DbSet<AuditoriaLog> AuditoriaLogs => Set<AuditoriaLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Aplica todas as configurações Fluent API da pasta Configurations automaticamente
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}
