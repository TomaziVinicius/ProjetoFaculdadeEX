using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TriagemImagem.Infrastructure.Data.Context;

namespace TriagemImagem.Api.Controllers;

[ApiController]
[Route("api/v1/database")]
public class DatabaseHealthController : ControllerBase
{
    private readonly AppDbContext _context;

    public DatabaseHealthController(AppDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Verifica a conectividade com o banco de dados MySQL e contagem de registros.
    /// </summary>
    [HttpGet("status")]
    public async Task<IActionResult> GetStatus()
    {
        try
        {
            var conectado = await _context.Database.CanConnectAsync();
            if (!conectado)
            {
                return StatusCode(503, new
                {
                    status = "Desconectado",
                    mensagem = "Não foi possível conectar ao banco de dados MySQL. Verifique o servidor local ou porta 3306."
                });
            }

            var totalUsuarios = await _context.Usuarios.CountAsync();
            var totalPacientes = await _context.Pacientes.CountAsync();
            var totalExames = await _context.Exames.CountAsync();

            return Ok(new
            {
                status = "Conectado",
                banco = "MySQL / MariaDB",
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
            return StatusCode(500, new
            {
                status = "Erro",
                mensagem = ex.Message
            });
        }
    }
}
