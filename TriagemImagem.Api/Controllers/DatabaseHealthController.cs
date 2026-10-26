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
    private readonly WorklistLabDbContext _contextLab;

    public DatabaseHealthController(AppDbContext context, WorklistLabDbContext contextLab)
    {
        _context = context;
        _contextLab = contextLab;
    }

    /// <summary>
    /// Verifica a conectividade com o Banco 1 (Aplicação/Triagem) e Banco 2 (Worklist/Laboratório).
    /// </summary>
    [HttpGet("status")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> GetStatus()
    {
        try
        {
            var conectadoBanco1 = await _context.Database.CanConnectAsync();
            var conectadoBanco2 = await _contextLab.Database.CanConnectAsync();

            if (!conectadoBanco1)
            {
                return StatusCode(StatusCodes.Status503ServiceUnavailable, new
                {
                    status = "Desconectado",
                    mensagem = "Não foi possível conectar ao Banco 1 (Aplicação/Triagem)."
                });
            }

            var totalUsuarios = await _context.Usuarios.CountAsync();
            var totalPacientes = await _context.Pacientes.CountAsync();
            var totalExames = await _context.Exames.CountAsync();
            var totalTriagens = await _context.FormulariosTriagem.CountAsync();
            var totalLogs = await _context.AuditoriaLogs.CountAsync();

            var provedor1 = _context.Database.ProviderName?.Contains("Sqlite") == true
                ? "SQLite (Modo Portátil Automático)"
                : "MySQL / MariaDB";

            var totalPedidosLab = conectadoBanco2 ? await _contextLab.Pedidos.CountAsync() : 0;
            var totalPacientesLab = conectadoBanco2 ? await _contextLab.Pacientes.CountAsync() : 0;
            var provedor2 = _contextLab.Database.ProviderName?.Contains("Sqlite") == true
                ? "SQLite (Modo Portátil Automático)"
                : "MySQL / MariaDB";

            var tiposFormularios = conectadoBanco2 
                ? await _contextLab.Pedidos.Select(p => p.TipoFormulario).Distinct().ToListAsync()
                : new List<string> { "RM_COLUNA", "RM_BRACO_ANTEBRACO", "RM_BACIA_QUADRIL", "TC_TORAX", "TC_ABDOMEN" };

            return Ok(new
            {
                status = "Conectado",
                banco1_aplicacao = new
                {
                    status = "Conectado",
                    provedor = provedor1,
                    usuarios = totalUsuarios,
                    pacientes = totalPacientes,
                    examesCadastrados = totalExames,
                    triagensSalvas = totalTriagens,
                    logsAuditoria = totalLogs
                },
                banco2_worklist_lab = new
                {
                    status = conectadoBanco2 ? "Conectado" : "Offline",
                    provedor = provedor2,
                    pedidosAgendados = totalPedidosLab,
                    pacientesCadastrados = totalPacientesLab,
                    formulariosDisponiveis = tiposFormularios
                },
                pastaDocumentos = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "TriagensDiagnosticoImagem"),
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
