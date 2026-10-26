using Microsoft.AspNetCore.Mvc;
using TriagemImagem.Application.DTOs;
using TriagemImagem.Application.Interfaces;

namespace TriagemImagem.Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[Produces("application/json")]
public class AuthController : ControllerBase
{
    private readonly IAuthAppService _authService;

    public AuthController(IAuthAppService authService)
    {
        _authService = authService;
    }

    /// <summary>
    /// Autentica o usuário técnico ou biomédico no posto de triagem.
    /// </summary>
    [HttpPost("login")]
    [ProducesResponseType(typeof(UsuarioAutenticadoDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginRequestDto request)
    {
        var usuario = await _authService.AutenticarAsync(request);
        if (usuario == null)
        {
            return Unauthorized(new { success = false, message = "Login ou senha inválidos." });
        }

        return Ok(usuario);
    }
}
