using TriagemImagem.Domain.Entities;
using TriagemImagem.Domain.Enums;
using TriagemImagem.Domain.Services;
using Xunit;

namespace TriagemImagem.Tests.Domain;

public class SemaforoClinicoTests
{
    private readonly SemaforoClinicoDomainService _service = new();

    [Fact]
    public void Avaliar_RespostasTodasNegativas_DeveRetornarVerdeELiberado()
    {
        // Arrange
        var respostas = new List<ItemRespostaTriagem>
        {
            new() { CodigoPergunta = "RM_MARCAPASSO", PerguntaEnunciado = "Portador de marca-passo?", RespostaValor = "NAO" },
            new() { CodigoPergunta = "RM_CLIPE_ANEURISMA", PerguntaEnunciado = "Clipe de aneurisma cerebral?", RespostaValor = "NAO" },
            new() { CodigoPergunta = "RM_CLAUSTROFOBIA", PerguntaEnunciado = "Claustrofobia?", RespostaValor = "NAO" }
        };

        // Act
        var resultado = _service.Avaliar(ModalidadeExame.RM, respostas, pesoKg: 75);

        // Assert
        Assert.True(resultado.IsVerde);
        Assert.False(resultado.BloqueiaExame);
        Assert.Empty(resultado.MotivosContraindicacao);
        Assert.Empty(resultado.AlertasAtencao);
    }

    [Fact]
    public void Avaliar_RMComMarcapasso_DeveRetornarVermelhoEBloquearExame()
    {
        // Arrange
        var respostas = new List<ItemRespostaTriagem>
        {
            new() { CodigoPergunta = "RM_MARCAPASSO", PerguntaEnunciado = "Portador de marca-passo?", RespostaValor = "SIM" },
            new() { CodigoPergunta = "RM_CLIPE_ANEURISMA", PerguntaEnunciado = "Clipe de aneurisma cerebral?", RespostaValor = "NAO" }
        };

        // Act
        var resultado = _service.Avaliar(ModalidadeExame.RM, respostas, pesoKg: 80);

        // Assert
        Assert.True(resultado.IsVermelho);
        Assert.True(resultado.BloqueiaExame);
        Assert.Single(resultado.MotivosContraindicacao);
        Assert.Contains("CONTRAINDICADA", resultado.MotivosContraindicacao.First());
    }

    [Fact]
    public void Avaliar_RMComClaustrofobia_DeveRetornarAmareloComAlerta()
    {
        // Arrange
        var respostas = new List<ItemRespostaTriagem>
        {
            new() { CodigoPergunta = "RM_MARCAPASSO", PerguntaEnunciado = "Portador de marca-passo?", RespostaValor = "NAO" },
            new() { CodigoPergunta = "RM_CLAUSTROFOBIA", PerguntaEnunciado = "Claustrofobia severa?", RespostaValor = "SIM" }
        };

        // Act
        var resultado = _service.Avaliar(ModalidadeExame.RM, respostas, pesoKg: 70);

        // Assert
        Assert.True(resultado.IsAmarelo);
        Assert.False(resultado.BloqueiaExame);
        Assert.Empty(resultado.MotivosContraindicacao);
        Assert.Single(resultado.AlertasAtencao);
        Assert.Contains("claustrofobia", resultado.AlertasAtencao.First(), StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Avaliar_TCComAlergiaIodo_DeveRetornarVermelhoEBloquear()
    {
        // Arrange
        var respostas = new List<ItemRespostaTriagem>
        {
            new() { CodigoPergunta = "TC_ALERGIA_IODO", PerguntaEnunciado = "Alergia prévia a contraste iodado?", RespostaValor = "SIM" },
            new() { CodigoPergunta = "TC_DIABETES_METFORMINA", PerguntaEnunciado = "Uso de Metformina?", RespostaValor = "NAO" }
        };

        // Act
        var resultado = _service.Avaliar(ModalidadeExame.TC, respostas, pesoKg: 65);

        // Assert
        Assert.True(resultado.IsVermelho);
        Assert.True(resultado.BloqueiaExame);
        Assert.Single(resultado.MotivosContraindicacao);
    }

    [Fact]
    public void Avaliar_TCComMetformina_DeveRetornarAmareloComInstrucaoDeSuspensao()
    {
        // Arrange
        var respostas = new List<ItemRespostaTriagem>
        {
            new() { CodigoPergunta = "TC_ALERGIA_IODO", PerguntaEnunciado = "Alergia prévia a contraste iodado?", RespostaValor = "NAO" },
            new() { CodigoPergunta = "TC_DIABETES_METFORMINA", PerguntaEnunciado = "Uso de Metformina?", RespostaValor = "SIM" }
        };

        // Act
        var resultado = _service.Avaliar(ModalidadeExame.TC, respostas, pesoKg: 85);

        // Assert
        Assert.True(resultado.IsAmarelo);
        Assert.False(resultado.BloqueiaExame);
        Assert.Single(resultado.AlertasAtencao);
        Assert.Contains("Metformina", resultado.AlertasAtencao.First());
    }
}
