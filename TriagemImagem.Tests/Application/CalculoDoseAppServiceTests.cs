using TriagemImagem.Application.DTOs;
using TriagemImagem.Application.Services;
using TriagemImagem.Domain.Enums;
using TriagemImagem.Domain.Exceptions;
using Xunit;

namespace TriagemImagem.Tests.Application;

public class CalculoDoseAppServiceTests
{
    private readonly CalculoDoseAppService _service = new();

    [Fact]
    public void CalcularDose_TC_ComPesoValido_DeveRetornarCalculoCorreto()
    {
        // Arrange
        var request = new CalcularDoseRequestDto
        {
            Modalidade = ModalidadeExame.TC,
            PesoKg = 80m,
            FatorMlPorKg = 1.5m
        };

        // Act
        var resultado = _service.CalcularDose(request);

        // Assert
        Assert.Equal(80m, resultado.PesoKg);
        Assert.Equal(ModalidadeExame.TC, resultado.Modalidade);
        Assert.Equal(120m, resultado.VolumeCalculadoMl);
        Assert.Equal(120m, resultado.VolumeAjustadoMl);
        Assert.False(resultado.TetoMaximoAtingido);
    }

    [Fact]
    public void CalcularDose_RM_ComPesoAlto_DeveAplicarTetoMaximo()
    {
        // Arrange
        var request = new CalcularDoseRequestDto
        {
            Modalidade = ModalidadeExame.RM,
            PesoKg = 150m,
            FatorMlPorKg = 0.2m // 150 * 0.2 = 30 mL > teto 25 mL
        };

        // Act
        var resultado = _service.CalcularDose(request);

        // Assert
        Assert.Equal(30m, resultado.VolumeCalculadoMl);
        Assert.Equal(25m, resultado.VolumeAjustadoMl);
        Assert.True(resultado.TetoMaximoAtingido);
        Assert.NotNull(resultado.AlertaClinico);
    }

    [Fact]
    public void CalcularDose_PesoInvalido_DeveLancarExcecao()
    {
        // Arrange
        var request = new CalcularDoseRequestDto
        {
            Modalidade = ModalidadeExame.TC,
            PesoKg = -5m
        };

        // Act & Assert
        Assert.Throws<DomainException>(() => _service.CalcularDose(request));
    }
}
