using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TriagemImagem.Domain.Entities;
using TriagemImagem.Domain.Enums;

namespace TriagemImagem.Infrastructure.Data.Configurations;

public class ExameConfiguration : IEntityTypeConfiguration<Exame>
{
    public void Configure(EntityTypeBuilder<Exame> builder)
    {
        builder.ToTable("exames");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Id)
               .HasColumnName("id")
               .ValueGeneratedOnAdd();

        builder.Property(e => e.PacienteId)
               .HasColumnName("paciente_id")
               .IsRequired();

        builder.Property(e => e.Atendimento)
               .HasColumnName("atendimento")
               .IsRequired();

        builder.Property(e => e.CodigoProcedimento)
               .HasColumnName("codigo_procedimento")
               .HasMaxLength(30)
               .IsRequired();

        builder.Property(e => e.SequenciaInterna)
               .HasColumnName("sequencia_interna")
               .IsRequired();

        builder.Property(e => e.DescricaoExame)
               .HasColumnName("descricao_exame")
               .HasMaxLength(150)
               .IsRequired();

        builder.Property(e => e.Modalidade)
               .HasColumnName("modalidade")
               .HasConversion<string>()
               .HasMaxLength(10)
               .IsRequired();

        builder.Property(e => e.Sala)
               .HasColumnName("sala")
               .HasMaxLength(20)
               .IsRequired();

        builder.Property(e => e.HorarioAgendado)
               .HasColumnName("horario_agendado")
               .IsRequired();

        builder.Property(e => e.HorarioChamada)
               .HasColumnName("horario_chamada");

        builder.Property(e => e.HorarioExecucao)
               .HasColumnName("horario_execucao");

        builder.Property(e => e.Status)
               .HasColumnName("status")
               .HasConversion<string>()
               .HasMaxLength(30)
               .HasDefaultValue(StatusExame.AGUARDANDO_TRIAGEM)
               .IsRequired();

        builder.Property(e => e.CriadoEm)
               .HasColumnName("criado_em")
               .HasDefaultValueSql("CURRENT_TIMESTAMP")
               .IsRequired();

        // Relacionamento com Paciente: Restrict para evitar exclusão em cascata
        builder.HasOne(e => e.Paciente)
               .WithMany()
               .HasForeignKey(e => e.PacienteId)
               .OnDelete(DeleteBehavior.Restrict);

        // Índices de otimização de consulta da Worklist
        builder.HasIndex(e => e.HorarioAgendado);
        builder.HasIndex(e => e.Status);
        builder.HasIndex(e => e.Modalidade);
    }
}
