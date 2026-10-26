using TriagemImagem.Application.Services;
using TriagemImagem.Domain.Entities;
using TriagemImagem.Domain.Enums;
using TriagemImagem.Domain.Services;
using Xunit;

namespace TriagemImagem.Tests.Application;

public class RelatorioTriagemAppServiceTests
{
    [Theory]
    [InlineData("RM_COLUNA", "QUESTIONÁRIO PARA RESSONÂNCIA DA COLUNA")]
    [InlineData("RM_BRACO_ANTEBRACO", "QUESTIONÁRIO PARA RESSONÂNCIA DE BRAÇO/ANTEBRAÇO")]
    [InlineData("RM_BACIA_QUADRIL", "QUESTIONÁRIO PARA RESSONÂNCIA DE BACIA/QUADRIL")]
    [InlineData("TC_TORAX", "QUESTIONÁRIO DE SEGURANÇA PARA TOMOGRAFIA DE TÓRAX")]
    public async Task GerarEArmazenarRelatorio_DeveGerarArquivoHtmlComCabecalhoCorreto(string tipoFormulario, string tituloEsperado)
    {
        // Arrange
        var service = new RelatorioTriagemAppService();
        var paciente = new Paciente
        {
            Id = 1,
            NomeCompleto = "Paciente Teste Relatório",
            DocumentoCpf = "12345678901",
            DataNascimento = new DateTime(1985, 5, 20),
            Sexo = "M",
            PesoKg = 75.0m
        };

        var exame = new Exame
        {
            Id = 10,
            Atendimento = 109999,
            Paciente = paciente,
            PacienteId = paciente.Id,
            Modalidade = tipoFormulario.StartsWith("TC") ? ModalidadeExame.TC : ModalidadeExame.RM,
            TipoFormulario = tipoFormulario,
            SegmentoLado = "Segmento Específico Teste",
            MotivoExame = "Investigação diagnóstica do paciente",
            Sala = "RM-01",
            DescricaoExame = "Exame de Teste Automatizado",
            Status = StatusExame.LIBERADO
        };

        var formulario = new FormularioTriagem
        {
            ExameId = exame.Id,
            Exame = exame,
            StatusRisco = StatusRiscoSemaforo.VERDE,
            VolumeContrasteMl = 15.0m,
            LoteContraste = "LOT-TESTE",
            IniciadoEm = DateTime.UtcNow.AddMinutes(-10),
            FinalizadoEm = DateTime.UtcNow,
            Respostas = new List<ItemRespostaTriagem>
            {
                new() { CodigoPergunta = "P1", PerguntaEnunciado = "Possui sintomas?", RespostaValor = "NAO", Impeditivo = false }
            }
        };

        var avaliacao = new ResultadoSemaforoClinico(StatusRiscoSemaforo.VERDE);

        // Act
        var caminhoArquivo = await service.GerarEArmazenarRelatorioAsync(exame, formulario, avaliacao, "Biomédico Responsável");

        // Assert
        Assert.NotNull(caminhoArquivo);
        Assert.True(File.Exists(caminhoArquivo));

        var conteudo = await File.ReadAllTextAsync(caminhoArquivo);
        Assert.Contains(tituloEsperado, conteudo);
        Assert.Contains("Instituto Maringá de Imagem", conteudo);
        Assert.Contains("Paciente Teste Relatório", conteudo);
        Assert.Contains("VERDE - LIBERADO PARA EXAME", conteudo);
        Assert.Contains("Biomédico Responsável", conteudo);
    }
}
