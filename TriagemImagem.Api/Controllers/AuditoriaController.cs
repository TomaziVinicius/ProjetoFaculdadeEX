using Microsoft.AspNetCore.Mvc;
using TriagemImagem.Application.DTOs;
using TriagemImagem.Application.Interfaces;

namespace TriagemImagem.Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[Produces("application/json")]
public class AuditoriaController : ControllerBase
{
    private readonly IAuditoriaAppService _auditoriaService;

    public AuditoriaController(IAuditoriaAppService auditoriaService)
    {
        _auditoriaService = auditoriaService;
    }

    /// <summary>
    /// Consulta os registros cronológicos da trilha de auditoria e segurança.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<AuditoriaLogDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Listar([FromQuery] int? limite = 50)
    {
        var limiteAjustado = Math.Clamp(limite ?? 50, 1, 200);
        var logs = await _auditoriaService.ObterUltimosLogsAsync(limiteAjustado);
        return Ok(logs);
    }
}
