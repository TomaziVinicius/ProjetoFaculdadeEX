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
            entity.Property(p => p.Id).HasColumnName("id");
            entity.Property(p => p.DocumentoCpf).HasColumnName("documento_cpf").HasMaxLength(11).IsRequired();
            entity.HasIndex(p => p.DocumentoCpf).IsUnique();
            entity.Property(p => p.NomeCompleto).HasColumnName("nome_completo").HasMaxLength(150).IsRequired();
            entity.Property(p => p.DataNascimento).HasColumnName("data_nascimento").IsRequired();
            entity.Property(p => p.Sexo).HasColumnName("sexo").HasMaxLength(1).IsRequired();
            entity.Property(p => p.PesoKg).HasColumnName("peso_kg").HasColumnType("decimal(5,2)");
            entity.Property(p => p.AlturaCm).HasColumnName("altura_cm");
            entity.Property(p => p.Telefone).HasColumnName("telefone").HasMaxLength(20);
            entity.Property(p => p.CriadoEm).HasColumnName("criado_em");
        });

        // 2. Mapeamento de Pedidos de Exames Agendados
        modelBuilder.Entity<WorklistPedidoLab>(entity =>
        {
            entity.ToTable("worklist_pedidos_exames");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.PacienteId).HasColumnName("paciente_id").IsRequired();
            entity.Property(e => e.Atendimento).HasColumnName("atendimento").IsRequired();
            entity.HasIndex(e => e.Atendimento).IsUnique();

            entity.Property(e => e.Modalidade).HasColumnName("modalidade").HasConversion<string>().HasMaxLength(10).IsRequired();
            entity.Property(e => e.CodigoProcedimento).HasColumnName("codigo_procedimento").HasMaxLength(30).IsRequired();
            entity.Property(e => e.DescricaoProcedimento).HasColumnName("descricao_procedimento").HasMaxLength(150).IsRequired();
            entity.Property(e => e.TipoFormulario).HasColumnName("tipo_formulario").HasMaxLength(50).IsRequired();
            entity.Property(e => e.SegmentoLado).HasColumnName("segmento_lado").HasMaxLength(50);
            entity.Property(e => e.PreparoAdequado).HasColumnName("preparo_adequado").IsRequired();
            entity.Property(e => e.PreparoMotivoInadequado).HasColumnName("preparo_motivo_inadequado").HasMaxLength(255);
            entity.Property(e => e.MotivoExame).HasColumnName("motivo_exame").HasColumnType("text");

            entity.Property(e => e.MedicoSolicitante).HasColumnName("medico_solicitante").HasMaxLength(100).IsRequired();
            entity.Property(e => e.CrmMedico).HasColumnName("crm_medico").HasMaxLength(20).IsRequired();
            entity.Property(e => e.Convenio).HasColumnName("convenio").HasMaxLength(50).IsRequired();
            entity.Property(e => e.Sala).HasColumnName("sala").HasMaxLength(20).IsRequired();
            entity.Property(e => e.HorarioAgendado).HasColumnName("horario_agendado").IsRequired();
            entity.Property(e => e.HorarioChamada).HasColumnName("horario_chamada");
            entity.Property(e => e.Status).HasColumnName("status").HasMaxLength(30).IsRequired();
            entity.Property(e => e.CriadoEm).HasColumnName("criado_em");

            entity.HasOne(e => e.Paciente)
                  .WithMany(p => p.Pedidos)
                  .HasForeignKey(e => e.PacienteId)
                  .OnDelete(DeleteBehavior.Restrict);
        });
    }
}
