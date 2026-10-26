using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TriagemImagem.Infrastructure.Data.Context;

namespace TriagemImagem.Api.Controllers;

[ApiController]
[Route("api/v1/database")]
[Produces("application/json")]
public class DatabaseHealthController : ControllerBase
{
    private readonly AppDbContext _context;

    public DatabaseHealthController(AppDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Verifica a conectividade com o banco de dados (MySQL ou SQLite portátil) e contagem de registros.
    /// </summary>
    [HttpGet("status")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> GetStatus()
    {
        try
        {
            var conectado = await _context.Database.CanConnectAsync();
            if (!conectado)
            {
                return StatusCode(StatusCodes.Status503ServiceUnavailable, new
                {
                    status = "Desconectado",
                    mensagem = "Não foi possível conectar ao banco de dados."
                });
            }

            var totalUsuarios = await _context.Usuarios.CountAsync();
            var totalPacientes = await _context.Pacientes.CountAsync();
            var totalExames = await _context.Exames.CountAsync();

            var provedor = _context.Database.ProviderName?.Contains("Sqlite") == true
                ? "SQLite (Modo Portátil Automático)"
                : "MySQL / MariaDB";

            return Ok(new
            {
                status = "Conectado",
                banco = provedor,
                metricas = new
                {
                    usuarios = totalUsuarios,
                    pacientes = totalPacientes,
                    examesAgendados = totalExames
                },
                horarioServidor = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, new
            {
                status = "Erro",
                mensagem = ex.Message
            });
        }
    }
}
