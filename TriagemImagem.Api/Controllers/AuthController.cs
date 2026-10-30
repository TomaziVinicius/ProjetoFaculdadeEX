using Microsoft.AspNetCore.Mvc;
using TriagemImagem.Application.DTOs;
using TriagemImagem.Application.Interfaces;
using TriagemImagem.Domain.Services;

namespace TriagemImagem.Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[Produces("application/json")]
public class AuthController : ControllerBase
{
    private readonly IAuthAppService _authService;
    private readonly ILdapService _ldapService;

    public AuthController(IAuthAppService authService, ILdapService ldapService)
    {
        _authService = authService;
        _ldapService = ldapService;
    }

    /// <summary>
    /// Autentica o usuário técnico ou biomédico no posto de triagem via Active Directory (LDAP) ou contingência local.
    /// </summary>
    [HttpPost("login")]
    [ProducesResponseType(typeof(UsuarioAutenticadoDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
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

    /// <summary>
    /// Testa a conectividade com o servidor Active Directory / LDAP e a existência do grupo obrigatório.
    /// </summary>
    [HttpGet("ldap/status")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> TestarLdap()
    {
        var (sucesso, mensagem, servidor, grupo) = await _ldapService.TestarConexaoAsync();
        if (!sucesso)
        {
            return StatusCode(StatusCodes.Status503ServiceUnavailable, new
            {
                sucesso = false,
                mensagem,
                servidor,
                grupo
            });
        }

        return Ok(new
        {
            sucesso = true,
            mensagem,
            servidor,
            grupo
        });
    }

    /// <summary>
    /// Lista os membros cadastrados no grupo GR_PROJ_FORMS no Active Directory.
    /// </summary>
    [HttpGet("ldap/membros")]
    [ProducesResponseType(typeof(List<LdapUsuarioInfo>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ListarMembrosGrupoLdap()
    {
        var membros = await _ldapService.ListarMembrosDoGrupoAsync();
        return Ok(new
        {
            total = membros.Count,
            membros
        });
    }
}
