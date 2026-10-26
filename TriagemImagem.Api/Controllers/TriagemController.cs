using Microsoft.AspNetCore.Mvc;
using TriagemImagem.Application.DTOs;
using TriagemImagem.Application.Interfaces;

namespace TriagemImagem.Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[Produces("application/json")]
public class TriagemController : ControllerBase
{
    private readonly ITriagemAppService _triagemService;
    private readonly ICalculoDoseAppService _calculoDoseService;

    public TriagemController(
        ITriagemAppService triagemService, 
        ICalculoDoseAppService calculoDoseService)
    {
        _triagemService = triagemService;
        _calculoDoseService = calculoDoseService;
    }

    /// <summary>
    /// Processa e salva o formulário de triagem, avaliando o Checklist da Aviação e definindo o Semáforo Clínico.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(DetalhesTriagemDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Salvar([FromBody] SalvarTriagemRequestDto request)
    {
        var resultado = await _triagemService.SalvarTriagemAsync(request);
        return Ok(resultado);
    }

    /// <summary>
    /// Obtém os dados e respostas da triagem associada a um exame.
    /// </summary>
    [HttpGet("exame/{exameId:int}")]
    [ProducesResponseType(typeof(DetalhesTriagemDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ObterPorExame(int exameId)
    {
        var triagem = await _triagemService.ObterPorExameIdAsync(exameId);
        if (triagem == null)
            return NotFound(new { success = false, message = "Triagem não encontrada para este exame." });

        return Ok(triagem);
    }

    /// <summary>
    /// Calcula a dosagem segura de meio de contraste (Iodado para TC / Gadolínio para RM) com base no peso e tetos clínicos.
    /// </summary>
    [HttpPost("calcular-dose")]
    [ProducesResponseType(typeof(ResultadoDoseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public IActionResult CalcularDose([FromBody] CalcularDoseRequestDto request)
    {
        var resultado = _calculoDoseService.CalcularDose(request);
        return Ok(resultado);
    }

    /// <summary>
    /// Concede liberação operacional para o exame ser executado na sala de TC ou RM.
    /// </summary>
    [HttpPost("{exameId:int}/liberar")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Liberar(int exameId, [FromBody] LiberarExameRequest request)
    {
        var liberado = await _triagemService.LiberarExameAsync(exameId, request.UsuarioId, request.Observacao);
        if (!liberado)
            return NotFound(new { success = false, message = "Exame não encontrado." });

        return Ok(new { success = true, message = "Exame liberado para execução." });
    }

    /// <summary>
    /// Bloqueia ou cancela o exame por contraindicação clínica impeditiva ou recusa.
    /// </summary>
    [HttpPost("{exameId:int}/cancelar")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Cancelar(int exameId, [FromBody] CancelarExameRequest request)
    {
        var cancelado = await _triagemService.CancelarExameAsync(exameId, request.UsuarioId, request.Motivo);
        if (!cancelado)
            return NotFound(new { success = false, message = "Exame não encontrado." });

        return Ok(new { success = true, message = "Exame cancelado com sucesso." });
    }
}

public class LiberarExameRequest
{
    public int UsuarioId { get; set; }
    public string? Observacao { get; set; }
}

public class CancelarExameRequest
{
    public int UsuarioId { get; set; }
    public string Motivo { get; set; } = string.Empty;
}
