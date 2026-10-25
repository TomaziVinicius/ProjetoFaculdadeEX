using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TriagemImagem.Domain.Entities;

namespace TriagemImagem.Infrastructure.Data.Configurations;

public class AuditoriaLogConfiguration : IEntityTypeConfiguration<AuditoriaLog>
{
    public void Configure(EntityTypeBuilder<AuditoriaLog> builder)
    {
        builder.ToTable("auditoria_logs");

        builder.HasKey(a => a.Id);

        builder.Property(a => a.Id)
               .HasColumnName("id")
               .ValueGeneratedOnAdd();

        builder.Property(a => a.UsuarioId)
               .HasColumnName("usuario_id")
               .IsRequired();

        builder.Property(a => a.ExameId)
               .HasColumnName("exame_id");

        builder.Property(a => a.TipoEvento)
               .HasColumnName("tipo_evento")
               .HasMaxLength(50)
               .IsRequired();

        builder.Property(a => a.Detalhes)
               .HasColumnName("detalhes")
               .HasColumnType("text")
               .IsRequired();

        builder.Property(a => a.IpOrigem)
               .HasColumnName("ip_origem")
               .HasMaxLength(45);

        builder.Property(a => a.CriadoEm)
               .HasColumnName("criado_em")
               .HasDefaultValueSql("CURRENT_TIMESTAMP")
               .IsRequired();

        builder.HasOne(a => a.Usuario)
               .WithMany()
               .HasForeignKey(a => a.UsuarioId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(a => a.CriadoEm);
    }
}
