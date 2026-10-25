using TriagemImagem.Domain.Exceptions;
using TriagemImagem.Domain.ValueObjects;
using Xunit;

namespace TriagemImagem.Tests.Domain;

public class CpfTests
{
    [Theory]
    [InlineData("52998224725", "529.982.247-25")]
    [InlineData("12345678909", "123.456.789-09")]
    public void CriarCpf_ComDigitosValidos_DeveInstanciarEFormatarCorretamente(string cpfEntrada, string formatadoEsperado)
    {
        // Act
        var cpf = new Cpf(cpfEntrada);

        // Assert
        Assert.Equal(cpfEntrada, cpf.Valor);
        Assert.Equal(formatadoEsperado, cpf.ValorFormatado);
    }

    [Theory]
    [InlineData("11111111111")] // Dígitos repetidos
    [InlineData("00000000000")]
    [InlineData("12345678900")] // Dígito verificador incorreto
    [InlineData("123")]          // Tamanho insuficiente
    [InlineData("")]             // Vazio
    [InlineData(null)]           // Nulo
    public void CriarCpf_ComValorInvalido_DeveLancarDomainException(string? cpfInvalido)
    {
        // Act & Assert
        Assert.Throws<DomainException>(() => new Cpf(cpfInvalido));
    }

    [Fact]
    public void Cpf_ComparacaoPorValor_DeveSerIgualParaMesmosDigitos()
    {
        // Arrange
        var cpf1 = new Cpf("529.982.247-25");
        var cpf2 = new Cpf("52998224725");

        // Assert
        Assert.Equal(cpf1, cpf2);
        Assert.True(cpf1.Equals(cpf2));
    }
}
