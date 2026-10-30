using Microsoft.AspNetCore.Mvc;
using TriagemImagem.Domain.Services;

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
    /// Consulta os exames do dia (ou da data selecionada) na view do Tasy (IMI_EXAMES_DIA_FORM_V).
    /// </summary>
    [HttpGet("exames-dia")]
    [ProducesResponseType(typeof(IReadOnlyList<ExameTasyDiaDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ConsultarExamesDia(
        [FromQuery] DateTime? data,
        [FromQuery] string? busca,
        [FromQuery] int limite = 50,
        [FromQuery] int offset = 0,
        CancellationToken cancellationToken = default)
    {
        var dataAlvo = data ?? DateTime.Today;
        var itens = await _tasyService.ConsultarExamesDiaAsync(dataAlvo, busca, limite, offset, cancellationToken);

        return Ok(new
        {
            dataConsultada = dataAlvo.ToString("yyyy-MM-dd"),
            total = itens.Count,
            itens
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
