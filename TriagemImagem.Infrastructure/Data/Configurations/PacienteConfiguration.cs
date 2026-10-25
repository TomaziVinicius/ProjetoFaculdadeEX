using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TriagemImagem.Domain.Entities;

namespace TriagemImagem.Infrastructure.Data.Configurations;

public class PacienteConfiguration : IEntityTypeConfiguration<Paciente>
{
    public void Configure(EntityTypeBuilder<Paciente> builder)
    {
        builder.ToTable("pacientes");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.Id)
               .HasColumnName("id")
               .ValueGeneratedOnAdd();

        builder.Property(p => p.DocumentoCpf)
               .HasColumnName("documento_cpf")
               .HasMaxLength(11)
               .IsRequired();

        builder.HasIndex(p => p.DocumentoCpf)
               .IsUnique();

        builder.Property(p => p.NomeCompleto)
               .HasColumnName("nome_completo")
               .HasMaxLength(100)
               .IsRequired();

        builder.Property(p => p.DataNascimento)
               .HasColumnName("data_nascimento")
               .HasColumnType("date")
               .IsRequired();

        builder.Property(p => p.Sexo)
               .HasColumnName("sexo")
               .HasMaxLength(1)
               .IsRequired();

        builder.Property(p => p.PesoKg)
               .HasColumnName("peso_kg")
               .HasPrecision(5, 2);

        builder.Property(p => p.AlturaCm)
               .HasColumnName("altura_cm");

        builder.Property(p => p.CriadoEm)
               .HasColumnName("criado_em")
               .HasDefaultValueSql("CURRENT_TIMESTAMP")
               .IsRequired();
    }
}
