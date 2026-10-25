using TriagemImagem.Domain.Entities;
using Xunit;

namespace TriagemImagem.Tests.Domain;

public class DomainUnitTests
{
    [Fact]
    public void Paciente_DeveCalcularIdadeCorretamente()
    {
        // Arrange
        var paciente = new Paciente
        {
            DataNascimento = DateTime.Today.AddYears(-30)
        };

        // Act
        var idade = paciente.ObterIdade();

        // Assert
        Assert.Equal(30, idade);
    }
}
