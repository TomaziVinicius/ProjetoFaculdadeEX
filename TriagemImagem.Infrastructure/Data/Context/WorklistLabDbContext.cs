using Microsoft.EntityFrameworkCore;
using TriagemImagem.Domain.Entities;

namespace TriagemImagem.Infrastructure.Data.Context;

/// <summary>
/// DbContext do Banco de Dados 2: Sistema Laboratorial de Agendamento (Worklist / RIS / HIS)
/// </summary>
public class WorklistLabDbContext : DbContext
{
    public WorklistLabDbContext(DbContextOptions<WorklistLabDbContext> options) : base(options)
    {
    }

    public DbSet<WorklistPacienteLab> Pacientes => Set<WorklistPacienteLab>();
    public DbSet<WorklistPedidoLab> Pedidos => Set<WorklistPedidoLab>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // 1. Mapeamento de Pacientes
        modelBuilder.Entity<WorklistPacienteLab>(entity =>
        {
            entity.ToTable("worklist_pacientes");
            entity.HasKey(p => p.Id);
            entity.Property(p => p.DocumentoCpf).HasMaxLength(11).IsRequired();
            entity.HasIndex(p => p.DocumentoCpf).IsUnique();
            entity.Property(p => p.NomeCompleto).HasMaxLength(150).IsRequired();
            entity.Property(p => p.Sexo).HasMaxLength(1).IsRequired();
            entity.Property(p => p.PesoKg).HasColumnType("decimal(5,2)");
            entity.Property(p => p.Telefone).HasMaxLength(20);
        });

        // 2. Mapeamento de Pedidos de Exames Agendados
        modelBuilder.Entity<WorklistPedidoLab>(entity =>
        {
            entity.ToTable("worklist_pedidos_exames");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Atendimento).IsRequired();
            entity.HasIndex(e => e.Atendimento).IsUnique();

            entity.Property(e => e.Modalidade).HasConversion<string>().HasMaxLength(10).IsRequired();
            entity.Property(e => e.CodigoProcedimento).HasMaxLength(30).IsRequired();
            entity.Property(e => e.DescricaoProcedimento).HasMaxLength(150).IsRequired();
            entity.Property(e => e.TipoFormulario).HasMaxLength(50).IsRequired();
            entity.Property(e => e.SegmentoLado).HasMaxLength(50);
            entity.Property(e => e.MotivoExame).HasColumnType("text");
            entity.Property(e => e.PreparoMotivoInadequado).HasMaxLength(255);

            entity.Property(e => e.MedicoSolicitante).HasMaxLength(100).IsRequired();
            entity.Property(e => e.CrmMedico).HasMaxLength(20).IsRequired();
            entity.Property(e => e.Convenio).HasMaxLength(50).IsRequired();
            entity.Property(e => e.Sala).HasMaxLength(20).IsRequired();
            entity.Property(e => e.Status).HasMaxLength(30).IsRequired();

            entity.HasOne(e => e.Paciente)
                  .WithMany(p => p.Pedidos)
                  .HasForeignKey(e => e.PacienteId)
                  .OnDelete(DeleteBehavior.Restrict);
        });
    }
}
