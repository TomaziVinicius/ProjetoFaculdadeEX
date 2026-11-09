using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TriagemImagem.Domain.Entities;
using TriagemImagem.Domain.Enums;
using TriagemImagem.Domain.Repositories;
using TriagemImagem.Domain.Services;
using TriagemImagem.Infrastructure.Data.Context;

namespace TriagemImagem.Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[Produces("application/json")]
public class TasyController : ControllerBase
{
    private readonly ITasyService _tasyService;
    private readonly ILogger<TasyController> _logger;

    public TasyController(ITasyService tasyService, ILogger<TasyController> logger)
    {
        _tasyService = tasyService;
        _logger = logger;
    }

    /// <summary>
    /// Testa a conectividade com o banco de dados Oracle do Tasy e afere tempo de resposta.
    /// </summary>
    [HttpGet("status")]
    [ProducesResponseType(typeof(TasyStatusDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> ObterStatusConexao(CancellationToken cancellationToken)
    {
        var status = await _tasyService.TestarConexaoAsync(cancellationToken);
        return Ok(status);
    }

    /// <summary>
    /// Consulta procedimentos internos cadastrados no Tasy com filtros de texto, código e paginação.
    /// </summary>
    [HttpGet("procedimentos")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> ConsultarProcedimentos(
        [FromQuery] string? busca,
        [FromQuery] long? codigo,
        [FromQuery] int limite = 50,
        [FromQuery] int offset = 0,
        CancellationToken cancellationToken = default)
    {
        var total = await _tasyService.ObterTotalProcedimentosAsync(busca, cancellationToken);
        var itens = await _tasyService.ConsultarProcedimentosAsync(busca, codigo, limite, offset, cancellationToken);

        return Ok(new
        {
            total,
            limite,
            offset,
            itens
        });
    }

    /// <summary>
    /// Consulta os exames do dia (ou da data selecionada) na view do Tasy (IMI_EXAMES_DIA_FORM_V),
    /// cruzando com os exames já importados, triados e liberados na base local de triagem.
    /// </summary>
    [HttpGet("exames-dia")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> ConsultarExamesDia(
        [FromQuery] DateTime? data,
        [FromQuery] string? busca,
        [FromServices] AppDbContext context,
        [FromQuery] int limite = 50,
        [FromQuery] int offset = 0,
        CancellationToken cancellationToken = default)
    {
        var dataAlvo = data ?? DateTime.Today;
        var itens = await _tasyService.ConsultarExamesDiaAsync(dataAlvo, busca, limite, offset, cancellationToken);

        // Cruza os atendimentos do Tasy com a base local MySQL de Triagem
        var atendimentos = itens.Select(i => (int)i.Atendimento).Distinct().ToList();
        var examesLocais = await context.Exames
            .Where(e => atendimentos.Contains(e.Atendimento))
            .ToListAsync(cancellationToken);

        var exameIds = examesLocais.Select(e => e.Id).ToList();
        var triagensLocais = await context.FormulariosTriagem
            .Where(f => exameIds.Contains(f.ExameId))
            .ToListAsync(cancellationToken);

        var itensEnriquecidos = itens.Select(item =>
        {
            var seq = (int)(item.SequenciaProcedimentoInterno ?? 1);
            var local = examesLocais.FirstOrDefault(e => 
                e.Atendimento == (int)item.Atendimento && 
                (item.SequenciaProcedimentoInterno.HasValue ? e.SequenciaInterna == seq : true));

            var triagem = local != null ? triagensLocais.FirstOrDefault(t => t.ExameId == local.Id) : null;
            var triado = local != null && (local.Status == StatusExame.LIBERADO || local.Status == StatusExame.BLOQUEADO_SEGURANCA);

            return new
            {
                atendimento = item.Atendimento,
                prontuario = item.Prontuario,
                pacienteNome = item.PacienteNome,
                dataNascimento = item.DataNascimento,
                idade = item.Idade,
                sexo = item.Sexo,
                cpf = item.Cpf,
                telefone = item.Telefone,
                dataEntrada = item.DataEntrada,
                prescricao = item.Prescricao,
                convenio = item.Convenio,
                medicoSolicitante = item.MedicoSolicitante,
                codigoProcedimento = item.CodigoProcedimento,
                procedimentoNome = item.ProcedimentoNome,
                sequenciaProcedimentoInterno = item.SequenciaProcedimentoInterno,
                nrSeqProcInterno = item.SequenciaProcedimentoInterno,
                setorAtendimento = item.SetorAtendimento,
                modalidade = item.Modalidade,
                statusExecucaoCodigo = item.StatusExecucaoCodigo,
                statusExecucaoDescricao = item.StatusExecucaoDescricao,
                // Dados sincronizados de Triagem:
                exameLocalId = local?.Id,
                statusTriagem = local?.Status.ToString(),
                triado,
                statusRiscoSemaforo = triagem?.StatusRisco.ToString(),
                caminhoArquivoArmazenado = triagem?.CaminhoArquivoArmazenado
            };
        });

        return Ok(new
        {
            dataConsultada = dataAlvo.ToString("yyyy-MM-dd"),
            total = itens.Count,
            itens = itensEnriquecidos
        });
    }

    /// <summary>
    /// Sincroniza e cadastra um exame localizado no Tasy na base de dados de triagem,
    /// garantindo que ele possua ID oficial para realização, avaliação de semáforo e emissão de laudo.
    /// </summary>
    [HttpPost("importar-exame")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ImportarExame(
        [FromBody] ImportarExameTasyRequest request,
        [FromServices] AppDbContext context,
        [FromServices] IAuditoriaRepository auditoriaRepo)
    {
        if (request == null || request.Atendimento <= 0)
            return BadRequest(new { success = false, message = "O número de atendimento é obrigatório." });

        if (string.IsNullOrWhiteSpace(request.PacienteNome))
            return BadRequest(new { success = false, message = "O nome do paciente é obrigatório." });

        var seqVal = request.NrSeqProcInterno ?? request.SequenciaProcedimentoInterno;
        var seqInterna = (int)(seqVal ?? 1);
        var atendimentoInt = (int)request.Atendimento;

        // 1. Verifica se o exame já foi importado anteriormente
        var seqHasVal = request.NrSeqProcInterno.HasValue || request.SequenciaProcedimentoInterno.HasValue;
        var existente = await context.Exames
            .Include(e => e.Paciente)
            .FirstOrDefaultAsync(e => e.Atendimento == atendimentoInt && 
                (seqHasVal ? e.SequenciaInterna == seqInterna : true));

        if (existente != null)
        {
            if (!string.IsNullOrWhiteSpace(request.Sala)) existente.Sala = request.Sala.Length > 20 ? request.Sala.Substring(0, 20) : request.Sala;
            if (!string.IsNullOrWhiteSpace(request.MedicoSolicitante)) existente.MedicoSolicitante = request.MedicoSolicitante.Length > 100 ? request.MedicoSolicitante.Substring(0, 100) : request.MedicoSolicitante;
            if (!string.IsNullOrWhiteSpace(request.Convenio)) existente.Convenio = request.Convenio.Length > 50 ? request.Convenio.Substring(0, 50) : request.Convenio;
            await context.SaveChangesAsync();

            var triagem = await context.FormulariosTriagem.FirstOrDefaultAsync(f => f.ExameId == existente.Id);

            return Ok(new
            {
                success = true,
                id = existente.Id,
                atendimento = existente.Atendimento,
                sequenciaInterna = existente.SequenciaInterna,
                sequenciaProcedimentoInterno = existente.SequenciaInterna,
                nrSeqProcInterno = existente.SequenciaInterna,
                pacienteId = existente.PacienteId,
                pacienteNome = existente.Paciente?.NomeCompleto ?? request.PacienteNome,
                status = existente.Status.ToString(),
                caminhoArquivoArmazenado = triagem?.CaminhoArquivoArmazenado,
                statusRiscoSemaforo = triagem?.StatusRisco.ToString(),
                message = "Exame já sincronizado anteriormente."
            });
        }

        // 2. Localiza ou cadastra o paciente
        var cpfLimpo = request.Cpf?.Replace(".", "").Replace("-", "").Replace(" ", "").Trim();
        if (cpfLimpo != null && cpfLimpo.Length > 11) cpfLimpo = cpfLimpo.Substring(0, 11);

        Paciente? paciente = null;
        if (!string.IsNullOrWhiteSpace(cpfLimpo) && cpfLimpo.Length == 11 && cpfLimpo != "00000000000")
        {
            paciente = await context.Pacientes.FirstOrDefaultAsync(p => p.DocumentoCpf == cpfLimpo);
        }

        if (paciente == null && !string.IsNullOrWhiteSpace(request.PacienteNome))
        {
            var nomeBusca = request.PacienteNome.Trim();
            paciente = await context.Pacientes.FirstOrDefaultAsync(p => p.NomeCompleto == nomeBusca);
        }

        if (paciente == null)
        {
            string cpfGerado;
            if (!string.IsNullOrWhiteSpace(cpfLimpo) && cpfLimpo.Length == 11 && cpfLimpo != "00000000000")
            {
                cpfGerado = cpfLimpo;
            }
            else
            {
                // Gera CPF de 11 dígitos garantidamente numérico e único
                cpfGerado = $"99{atendimentoInt.ToString().PadLeft(9, '0')}";
                if (cpfGerado.Length > 11) cpfGerado = cpfGerado.Substring(0, 11);

                int counter = 0;
                while (await context.Pacientes.AnyAsync(p => p.DocumentoCpf == cpfGerado))
                {
                    counter++;
                    cpfGerado = $"98{counter.ToString().PadLeft(2, '0')}{atendimentoInt.ToString().PadLeft(7, '0')}";
                    if (cpfGerado.Length > 11) cpfGerado = cpfGerado.Substring(0, 11);
                }
            }

            var dataNasc = request.DataNascimento 
                ?? (request.Idade.HasValue && request.Idade > 0 ? DateTime.Today.AddYears(-request.Idade.Value) : DateTime.Today.AddYears(-35));

            var nomeComp = request.PacienteNome.Trim();
            if (nomeComp.Length > 100) nomeComp = nomeComp.Substring(0, 100);

            var sexo = string.IsNullOrWhiteSpace(request.Sexo) ? "M" : request.Sexo.Trim().ToUpperInvariant();
            if (sexo.Length > 1) sexo = sexo.Substring(0, 1);
            if (sexo != "M" && sexo != "F") sexo = "M";

            paciente = new Paciente
            {
                NomeCompleto = nomeComp,
                DocumentoCpf = cpfGerado,
                DataNascimento = dataNasc,
                Sexo = sexo,
                PesoKg = request.PesoKg ?? 70m,
                AlturaCm = request.AlturaCm ?? 170,
                CriadoEm = DateTime.UtcNow
            };
            await context.Pacientes.AddAsync(paciente);
            await context.SaveChangesAsync();
        }

        // 3. Cadastra o Exame na base de triagem
        var modalidade = (request.Modalidade?.Equals("TC", StringComparison.OrdinalIgnoreCase) ?? false)
            ? ModalidadeExame.TC
            : ModalidadeExame.RM;

        var tipoForm = string.IsNullOrWhiteSpace(request.TipoFormulario)
            ? (modalidade == ModalidadeExame.TC ? "TC_TORAX" : "RM_COLUNA")
            : request.TipoFormulario.Trim();
        if (tipoForm.Length > 50) tipoForm = tipoForm.Substring(0, 50);

        var descExame = string.IsNullOrWhiteSpace(request.ProcedimentoNome)
            ? (modalidade == ModalidadeExame.TC ? "Tomografia Computadorizada" : "Ressonância Magnética")
            : request.ProcedimentoNome.Trim();
        if (descExame.Length > 150) descExame = descExame.Substring(0, 150);

        var codProc = request.CodigoProcedimento?.ToString() ?? "41101999";
        if (codProc.Length > 30) codProc = codProc.Substring(0, 30);

        var segmento = string.IsNullOrWhiteSpace(request.SegmentoLado) ? "Padrão" : request.SegmentoLado.Trim();
        if (segmento.Length > 50) segmento = segmento.Substring(0, 50);

        var medico = string.IsNullOrWhiteSpace(request.MedicoSolicitante) ? "Médico Solicitante Tasy" : request.MedicoSolicitante.Trim();
        if (medico.Length > 100) medico = medico.Substring(0, 100);

        var convenio = string.IsNullOrWhiteSpace(request.Convenio) ? "Particular / Tasy" : request.Convenio.Trim();
        if (convenio.Length > 50) convenio = convenio.Substring(0, 50);

        var sala = string.IsNullOrWhiteSpace(request.Sala) ? (modalidade == ModalidadeExame.TC ? "TC-01" : "RM-01") : request.Sala.Trim().ToUpperInvariant();
        if (sala.Length > 20) sala = sala.Substring(0, 20);

        var exame = new Exame
        {
            PacienteId = paciente.Id,
            Atendimento = atendimentoInt,
            CodigoProcedimento = codProc,
            SequenciaInterna = seqInterna,
            DescricaoExame = descExame,
            Modalidade = modalidade,
            TipoFormulario = tipoForm,
            SegmentoLado = segmento,
            MotivoExame = string.IsNullOrWhiteSpace(request.IndicacaoClinica) ? "Exame prescrito e importado do Oracle Tasy" : request.IndicacaoClinica.Trim(),
            MedicoSolicitante = medico,
            Convenio = convenio,
            Sala = sala,
            HorarioAgendado = request.DataEntrada ?? DateTime.Now,
            Status = StatusExame.AGUARDANDO_TRIAGEM,
            CriadoEm = DateTime.UtcNow
        };

        await context.Exames.AddAsync(exame);
        await context.SaveChangesAsync();

        // 4. Registro de auditoria
        await auditoriaRepo.RegistrarLogAsync(new AuditoriaLog
        {
            ExameId = exame.Id,
            UsuarioId = 1,
            TipoEvento = "EXAME_IMPORTADO_TASY",
            Detalhes = $"Exame importado do Tasy: Atendimento #{exame.Atendimento} (Seq {exame.SequenciaInterna}) para '{paciente.NomeCompleto}' ({exame.DescricaoExame})."
        });

        return Ok(new
        {
            success = true,
            id = exame.Id,
            atendimento = exame.Atendimento,
            sequenciaInterna = exame.SequenciaInterna,
            sequenciaProcedimentoInterno = exame.SequenciaInterna,
            nrSeqProcInterno = exame.SequenciaInterna,
            pacienteId = paciente.Id,
            pacienteNome = paciente.NomeCompleto,
            status = exame.Status.ToString(),
            message = $"Exame #{exame.Atendimento} sincronizado e pronto para triagem!"
        });
    }

    /// <summary>
    /// Identifica o IP e a estação de trabalho conectada para associação de sala operacional (ex: 192.1.2.120 -> IMI-6691).
    /// </summary>
    [HttpGet("workstation")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult ObterInfoWorkstation()
    {
        var ip = HttpContext.Request.Headers["X-Forwarded-For"].FirstOrDefault()
                 ?? HttpContext.Connection.RemoteIpAddress?.ToString()
                 ?? "127.0.0.1";

        // Mapeamento de computadores/estações conhecidas para a sala operacional
        var mapaEstacoes = new Dictionary<string, (string Sala, string Hostname)>(StringComparer.OrdinalIgnoreCase)
        {
            ["192.1.2.120"] = ("RM-01", "IMI-6691"),
            ["127.0.0.1"] = ("RM-01", "LOCALHOST-DEV"),
            ["::1"] = ("RM-01", "LOCALHOST-DEV")
        };

        var sala = "RM-01";
        var hostname = "ESTACAO-CLIENTE";

        if (mapaEstacoes.TryGetValue(ip, out var info))
        {
            sala = info.Sala;
            hostname = info.Hostname;
        }

        return Ok(new
        {
            ip,
            hostname,
            salaPadrao = sala,
            descricao = $"{sala} ({ip} - {hostname})"
        });
    }

    /// <summary>
    /// Executa consultas SELECT customizadas no Oracle Tasy para validação e auditoria da TI.
    /// Operações de alteração ou exclusão são expressamente bloqueadas.
    /// </summary>
    [HttpPost("query")]
    [ProducesResponseType(typeof(TasyQueryResultDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ExecutarSelect([FromBody] ExecutarQueryRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request?.Query))
        {
            return BadRequest(new { mensagem = "A consulta SQL não pode ser vazia." });
        }

        var resultado = await _tasyService.ExecutarSelectCustomizadoAsync(
            request.Query,
            request.Limite ?? 100,
            cancellationToken);

        return Ok(resultado);
    }
}

public sealed record ExecutarQueryRequest(string Query, int? Limite);

public sealed record ImportarExameTasyRequest(
    long Atendimento,
    long? SequenciaProcedimentoInterno,
    string PacienteNome,
    string? Cpf,
    int? Idade,
    DateTime? DataNascimento,
    string? Sexo,
    string? Telefone,
    string ProcedimentoNome,
    long? CodigoProcedimento,
    string? MedicoSolicitante,
    string? Convenio,
    string? Sala,
    string? Modalidade,
    string? TipoFormulario,
    string? SegmentoLado,
    string? IndicacaoClinica,
    DateTime? DataEntrada,
    decimal? PesoKg,
    int? AlturaCm,
    long? NrSeqProcInterno = null
);

