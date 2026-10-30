using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using TriagemImagem.Domain.Configurations;
using TriagemImagem.Domain.Services;
using TriagemImagem.Infrastructure.Services;
using Xunit;
using Xunit.Abstractions;

namespace TriagemImagem.Tests;

public class OracleTasyConnectionTests
{
    private readonly ITestOutputHelper _output;
    private readonly ITasyService _tasyService;

    public OracleTasyConnectionTests(ITestOutputHelper output)
    {
        _output = output;

        var settings = new TasySettings
        {
            Host = "192.1.1.12",
            Port = 1521,
            ServiceName = "tasytst",
            User = "IMI_FORMULARIO",
            Password = "(q)qiS6*lupKu)}*",
            ConnectionTimeoutSeconds = 10,
            ProcedimentosView = "TASY.IMI_EXAMES_PROC_INTERNO_V"
        };

        var options = Options.Create(settings);
        _tasyService = new TasyService(options, NullLogger<TasyService>.Instance);
    }

    [Fact]
    public async Task TasyService_TestarConexao_RetornaSucessoELatencia()
    {
        var status = await _tasyService.TestarConexaoAsync();

        _output.WriteLine($"Conectado: {status.Conectado}");
        _output.WriteLine($"Latência: {status.LatenciaMs}ms");
        _output.WriteLine($"Versão: {status.VersaoOracle}");
        _output.WriteLine($"Total Procedimentos: {status.TotalProcedimentos}");

        Assert.True(status.Conectado);
        Assert.True(status.TotalProcedimentos > 0);
        Assert.Contains("Oracle", status.VersaoOracle);
    }

    [Fact]
    public async Task TasyService_ConsultarProcedimentos_ComFiltro_RetornaProcedimentos()
    {
        // 1. Consulta geral paginada
        var todos = await _tasyService.ConsultarProcedimentosAsync(limite: 10);
        Assert.NotEmpty(todos);
        Assert.True(todos.Count <= 10);

        _output.WriteLine($"Primeiro procedimento: {todos[0].Sequencia} - {todos[0].Descricao} (Modalidade: {todos[0].ModalidadeEstimada})");

        // 2. Consulta filtrada por texto
        var ressonancias = await _tasyService.ConsultarProcedimentosAsync(busca: "COLUNA", limite: 5);
        Assert.NotEmpty(ressonancias);
        foreach (var r in ressonancias)
        {
            _output.WriteLine($" - {r.Descricao} [{r.ModalidadeEstimada}]");
            Assert.Contains("COLUNA", r.Descricao, StringComparison.OrdinalIgnoreCase);
        }

        // 3. Consulta filtrada por código
        var codigoAlvo = todos[0].CodigoProcedimento;
        if (codigoAlvo.HasValue)
        {
            var porCodigo = await _tasyService.ConsultarProcedimentosAsync(codigo: codigoAlvo.Value);
            Assert.NotEmpty(porCodigo);
        }
    }

    [Fact]
    public async Task TasyService_ExecutarSelectCustomizado_ValidaComandoComSeguranca()
    {
        // 1. SELECT válido
        var sqlValido = "SELECT NR_SEQUENCIA, CD_PROCEDIMENTO, DS_PROC_EXAME FROM TASY.IMI_EXAMES_PROC_INTERNO_V WHERE ROWNUM <= 3";
        var resultado = await _tasyService.ExecutarSelectCustomizadoAsync(sqlValido);

        Assert.True(resultado.Sucesso);
        Assert.Equal(3, resultado.TotalLinhas);
        Assert.Contains("NR_SEQUENCIA", resultado.Colunas);

        // 2. Tentativa de comando proibido (ex: DELETE)
        var sqlProibido = "DELETE FROM TASY.IMI_EXAMES_PROC_INTERNO_V";
        var resultadoProibido = await _tasyService.ExecutarSelectCustomizadoAsync(sqlProibido);

        Assert.False(resultadoProibido.Sucesso);
        Assert.Contains("Apenas instruções de consulta", resultadoProibido.MensagemErro);

        // 3. Tentativa de injeção DDL
        var sqlDrop = "SELECT 1 FROM DUAL; DROP TABLE TESTE";
        var resultadoDrop = await _tasyService.ExecutarSelectCustomizadoAsync(sqlDrop);
        Assert.False(resultadoDrop.Sucesso);
    }

    [Fact]
    public async Task TasyService_Inspecionar_ImiExamesDiaFormV()
    {
        var sqlView = @"
            SELECT 
                NR_ATENDIMENTO,
                CD_PESSOA_FISICA,
                NM_PESSOA_FISICA,
                DT_NASCIMENTO,
                IE_SEXO,
                NR_CPF,
                NR_TELEFONE_CELULAR,
                DT_ENTRADA,
                NR_PRESCRICAO,
                CD_PROCEDIMENTO,
                NR_SEQ_PROC_INTERNO,
                CD_SETOR_ATENDIMENTO,
                DS_PROCEDIMENTO,
                DS_MEDICO_SOLICITANTE,
                DS_CONVENIO,
                IE_STATUS_EXECUCAO
            FROM TASY.IMI_EXAMES_DIA_FORM_V
            WHERE ROWNUM <= 5";

        var resView = await _tasyService.ExecutarSelectCustomizadoAsync(sqlView, limiteMaximo: 5);
        Assert.True(resView.Sucesso, $"Erro ao consultar view: {resView.MensagemErro}");
        Assert.NotEmpty(resView.Linhas);
        Assert.Contains("DS_PROCEDIMENTO", resView.Colunas);
        Assert.Contains("DS_MEDICO_SOLICITANTE", resView.Colunas);
        Assert.Contains("DS_CONVENIO", resView.Colunas);
        Assert.Contains("IE_STATUS_EXECUCAO", resView.Colunas);

        _output.WriteLine($"View TASY.IMI_EXAMES_DIA_FORM_V validada com sucesso! ({resView.TotalLinhas} linhas testadas)");
        foreach (var l in resView.Linhas)
        {
            _output.WriteLine($"Atend: {l["NR_ATENDIMENTO"]} | DT_ENTRADA: {l["DT_ENTRADA"]} ({l["DT_ENTRADA"]?.GetType().Name}) | Paciente: {l["NM_PESSOA_FISICA"]} | Exame: {l["DS_PROCEDIMENTO"]} | Status: {l["IE_STATUS_EXECUCAO"]}");
        }
    }

    [Fact]
    public async Task TasyService_ConsultarExamesDia_RetornaExamesFormatados()
    {
        // Testa consulta com data existente na base tasytst (2014-05-30)
        var examesData = await _tasyService.ConsultarExamesDiaAsync(
            data: new DateTime(2014, 5, 30),
            limite: 10);

        _output.WriteLine($"Total retornado para 2014-05-30: {examesData.Count}");
        foreach (var ex in examesData)
        {
            _output.WriteLine($"[{ex.Atendimento}] {ex.PacienteNome} ({ex.Idade} anos, {ex.Sexo}) - {ex.ProcedimentoNome} | Médico: {ex.MedicoSolicitante} | Convênio: {ex.Convenio} | Status: {ex.StatusExecucaoDescricao} ({ex.StatusExecucaoCodigo}) | Setor: {ex.SetorAtendimento}");
        }

        Assert.NotEmpty(examesData);
        Assert.All(examesData, ex => Assert.Equal("RM", ex.Modalidade));

        // Testa busca por nome
        var buscaNome = await _tasyService.ConsultarExamesDiaAsync(
            data: new DateTime(2014, 5, 30),
            busca: "Anderson");

        Assert.Single(buscaNome);
        Assert.Equal(367238, buscaNome[0].Atendimento);
        Assert.Contains("Anderson", buscaNome[0].PacienteNome);
    }
}


