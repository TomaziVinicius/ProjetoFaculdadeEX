using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TriagemImagem.Domain.Entities;

namespace TriagemImagem.Infrastructure.Data.Configurations;

public class ItemRespostaTriagemConfiguration : IEntityTypeConfiguration<ItemRespostaTriagem>
{
    public void Configure(EntityTypeBuilder<ItemRespostaTriagem> builder)
    {
        builder.ToTable("itens_resposta_triagem");

        builder.HasKey(i => i.Id);

        builder.Property(i => i.Id)
               .HasColumnName("id")
               .ValueGeneratedOnAdd();

        builder.Property(i => i.FormularioId)
               .HasColumnName("formulario_id")
               .IsRequired();

        builder.Property(i => i.CodigoPergunta)
               .HasColumnName("codigo_pergunta")
               .HasMaxLength(50)
               .IsRequired();

        builder.Property(i => i.PerguntaEnunciado)
               .HasColumnName("pergunta_enunciado")
               .HasMaxLength(255)
               .IsRequired();

        builder.Property(i => i.RespostaValor)
               .HasColumnName("resposta_valor")
               .HasMaxLength(100)
               .IsRequired();

        builder.Property(i => i.Impeditivo)
               .HasColumnName("impeditivo")
               .HasDefaultValue(false)
               .IsRequired();

        builder.Property(i => i.Justificativa)
               .HasColumnName("justificativa")
               .HasMaxLength(255);
    }
}
