using Microsoft.AspNetCore.Mvc;
using TriagemImagem.Application.DTOs;
using TriagemImagem.Application.Interfaces;

namespace TriagemImagem.Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[Produces("application/json")]
public class WorklistController : ControllerBase
{
    private readonly IWorklistAppService _worklistService;

    public WorklistController(IWorklistAppService worklistService)
    {
        _worklistService = worklistService;
    }

    /// <summary>
    /// Lista os agendamentos de TC e RM do dia com suporte a filtros e busca textual.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<ExameWorklistDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Listar([FromQuery] FiltroWorklistDto filtro)
    {
        var exames = await _worklistService.ObterWorklistAsync(filtro);
        return Ok(exames);
    }

    /// <summary>
    /// Obtém os dados completos de um exame específico da fila de trabalho.
    /// </summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ExameWorklistDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ObterPorId(int id)
    {
        var exame = await _worklistService.ObterExamePorIdAsync(id);
        if (exame == null)
            return NotFound(new { success = false, message = "Exame não encontrado." });

        return Ok(exame);
    }

    /// <summary>
    /// Registra a chamada do paciente para a sala de exame.
    /// </summary>
    [HttpPost("{id:int}/chamar")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ChamarPaciente(int id, [FromBody] ChamarPacienteRequest request)
    {
        var chamado = await _worklistService.ChamarPacienteAsync(id, request.UsuarioId);
        if (!chamado)
            return NotFound(new { success = false, message = "Exame não encontrado." });

        return Ok(new { success = true, message = "Paciente chamado com sucesso." });
    }
}

public class ChamarPacienteRequest
{
    public int UsuarioId { get; set; }
}
