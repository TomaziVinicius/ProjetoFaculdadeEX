using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TriagemImagem.Domain.Entities;
using TriagemImagem.Domain.Enums;

namespace TriagemImagem.Infrastructure.Data.Configurations;

public class FormularioTriagemConfiguration : IEntityTypeConfiguration<FormularioTriagem>
{
    public void Configure(EntityTypeBuilder<FormularioTriagem> builder)
    {
        builder.ToTable("formularios_triagem");

        builder.HasKey(f => f.Id);

        builder.Property(f => f.Id)
               .HasColumnName("id")
               .ValueGeneratedOnAdd();

        builder.Property(f => f.ExameId)
               .HasColumnName("exame_id")
               .IsRequired();

        builder.HasIndex(f => f.ExameId)
               .IsUnique();

        builder.Property(f => f.UsuarioResponsavelId)
               .HasColumnName("usuario_responsavel_id")
               .IsRequired();

        builder.Property(f => f.VolumeContrasteMl)
               .HasColumnName("volume_contraste_ml")
               .HasPrecision(5, 2);

        builder.Property(f => f.LoteContraste)
               .HasColumnName("lote_contraste")
               .HasMaxLength(50);

        builder.Property(f => f.CaminhoArquivoArmazenado)
               .HasColumnName("caminho_arquivo_armazenado")
               .HasMaxLength(255);

        builder.Property(f => f.StatusRisco)
               .HasColumnName("status_risco")
               .HasConversion<string>()
               .HasMaxLength(20)
               .IsRequired();

        builder.Property(f => f.ObservacoesClinicas)
               .HasColumnName("observacoes_clinicas")
               .HasColumnType("text");

        builder.Property(f => f.IniciadoEm)
               .HasColumnName("iniciado_em")
               .IsRequired();

        builder.Property(f => f.FinalizadoEm)
               .HasColumnName("finalizado_em");

        // Regras de Integridade: Restrict nos relacionamentos
        builder.HasOne(f => f.Exame)
               .WithOne()
               .HasForeignKey<FormularioTriagem>(f => f.ExameId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(f => f.UsuarioResponsavel)
               .WithMany()
               .HasForeignKey(f => f.UsuarioResponsavelId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(f => f.Respostas)
               .WithOne(r => r.Formulario)
               .HasForeignKey(r => r.FormularioId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}
