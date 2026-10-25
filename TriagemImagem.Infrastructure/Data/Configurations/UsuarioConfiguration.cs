using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TriagemImagem.Domain.Entities;

namespace TriagemImagem.Infrastructure.Data.Configurations;

public class UsuarioConfiguration : IEntityTypeConfiguration<Usuario>
{
    public void Configure(EntityTypeBuilder<Usuario> builder)
    {
        builder.ToTable("usuarios");

        builder.HasKey(u => u.Id);

        builder.Property(u => u.Id)
               .HasColumnName("id")
               .ValueGeneratedOnAdd();

        builder.Property(u => u.Nome)
               .HasColumnName("nome")
               .HasMaxLength(100)
               .IsRequired();

        builder.Property(u => u.Login)
               .HasColumnName("login")
               .HasMaxLength(50)
               .IsRequired();

        builder.HasIndex(u => u.Login)
               .IsUnique();

        builder.Property(u => u.SenhaUsuario)
               .HasColumnName("senha_usuario")
               .HasMaxLength(255)
               .IsRequired();

        builder.Property(u => u.Perfil)
               .HasColumnName("perfil")
               .HasConversion<string>()
               .HasMaxLength(20)
               .IsRequired();

        builder.Property(u => u.StatusUsuario)
               .HasColumnName("status_usuario")
               .HasDefaultValue(true)
               .IsRequired();

        builder.Property(u => u.CriadoEm)
               .HasColumnName("criado_em")
               .HasDefaultValueSql("CURRENT_TIMESTAMP")
               .IsRequired();
    }
}
