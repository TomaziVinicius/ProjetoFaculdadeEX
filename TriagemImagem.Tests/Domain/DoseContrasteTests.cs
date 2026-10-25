using TriagemImagem.Domain.Enums;
using TriagemImagem.Domain.Exceptions;
using TriagemImagem.Domain.ValueObjects;
using Xunit;

namespace TriagemImagem.Tests.Domain;

public class DoseContrasteTests
{
    [Fact]
    public void CalcularParaTC_Paciente70Kg_DeveCalcular105MlSemTeto()
    {
        // Arrange
        var peso = 70.0m;

        // Act
        var dose = DoseContraste.CalcularParaTC(peso, 1.5m);

        // Assert
        Assert.Equal(105.0m, dose.VolumeCalculadoMl);
        Assert.Equal(105.0m, dose.VolumeAjustadoMl);
        Assert.False(dose.TetoMaximoAtingido);
        Assert.Null(dose.AlertaClinico);
        Assert.Equal(ModalidadeExame.TC, dose.Modalidade);
    }

    [Fact]
    public void CalcularParaTC_Paciente120Kg_DeveAplicarTetoMaximo150Ml()
    {
        // Arrange
        var peso = 120.0m; // 120 * 1.5 = 180.0 mL > 150 mL

        // Act
        var dose = DoseContraste.CalcularParaTC(peso, 1.5m);

        // Assert
        Assert.Equal(180.0m, dose.VolumeCalculadoMl);
        Assert.Equal(150.0m, dose.VolumeAjustadoMl);
        Assert.True(dose.TetoMaximoAtingido);
        Assert.NotNull(dose.AlertaClinico);
        Assert.Contains("teto seguro", dose.AlertaClinico);
    }

    [Fact]
    public void CalcularParaRM_Paciente70Kg_DeveCalcular14MlSemTeto()
    {
        // Arrange
        var peso = 70.0m;

        // Act
        var dose = DoseContraste.CalcularParaRM(peso, 0.2m);

        // Assert
        Assert.Equal(14.0m, dose.VolumeCalculadoMl);
        Assert.Equal(14.0m, dose.VolumeAjustadoMl);
        Assert.False(dose.TetoMaximoAtingido);
        Assert.Null(dose.AlertaClinico);
        Assert.Equal(ModalidadeExame.RM, dose.Modalidade);
    }

    [Fact]
    public void CalcularParaRM_Paciente150Kg_DeveAplicarTetoMaximo25Ml()
    {
        // Arrange
        var peso = 150.0m; // 150 * 0.2 = 30.0 mL > 25 mL

        // Act
        var dose = DoseContraste.CalcularParaRM(peso, 0.2m);

        // Assert
        Assert.Equal(30.0m, dose.VolumeCalculadoMl);
        Assert.Equal(25.0m, dose.VolumeAjustadoMl);
        Assert.True(dose.TetoMaximoAtingido);
        Assert.NotNull(dose.AlertaClinico);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-10)]
    [InlineData(450)]
    public void CalcularDose_PesoInvalido_DeveLancarDomainException(decimal pesoInvalido)
    {
        // Act & Assert
        Assert.Throws<DomainException>(() => DoseContraste.CalcularParaTC(pesoInvalido));
        Assert.Throws<DomainException>(() => DoseContraste.CalcularParaRM(pesoInvalido));
    }
}
