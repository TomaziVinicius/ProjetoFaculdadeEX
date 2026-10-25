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

        modelBuilder.Entity<Usuario>().ToTable("usuarios");
        modelBuilder.Entity<Paciente>().ToTable("pacientes");
        modelBuilder.Entity<Exame>().ToTable("exames");
        modelBuilder.Entity<FormularioTriagem>().ToTable("formularios_triagem");
        modelBuilder.Entity<ItemRespostaTriagem>().ToTable("itens_resposta_triagem");
        modelBuilder.Entity<AuditoriaLog>().ToTable("auditoria_logs");
    }
}
