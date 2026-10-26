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
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
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
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ObterPorId(int id)
    {
        if (id <= 0)
            return BadRequest(new { success = false, message = "O identificador do exame deve ser um número inteiro positivo maior que zero." });

        var exame = await _worklistService.ObterExamePorIdAsync(id);
        if (exame == null)
            return NotFound(new { success = false, message = "Exame não encontrado na fila de trabalho." });

        return Ok(exame);
    }

    /// <summary>
    /// Registra a chamada do paciente para a sala de exame.
    /// </summary>
    [HttpPost("{id:int}/chamar")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ChamarPaciente(int id, [FromBody] ChamarPacienteRequest request)
    {
        if (id <= 0)
            return BadRequest(new { success = false, message = "O identificador do exame deve ser um número inteiro positivo maior que zero." });

        if (request == null || request.UsuarioId <= 0)
            return BadRequest(new { success = false, message = "É obrigatório informar o identificador do operador responsável pela chamada." });

        var chamado = await _worklistService.ChamarPacienteAsync(id, request.UsuarioId);
        if (!chamado)
            return NotFound(new { success = false, message = "Exame não encontrado para chamada." });

        return Ok(new { success = true, message = "Paciente chamado com sucesso." });
    }

    /// <summary>
    /// Consulta os agendamentos diretamente no Banco de Dados 2 (Laboratório / HIS-RIS Worklist).
    /// Demonstra a integração entre o sistema hospitalar e a estação de triagem.
    /// </summary>
    [HttpGet("laboratorio")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> ListarLaboratorio([FromServices] TriagemImagem.Domain.Repositories.IWorklistLabRepository labRepo)
    {
        var pedidos = await labRepo.ListarPedidosAsync();
        return Ok(pedidos.Select(p => new
        {
            atendimento = p.Atendimento,
            paciente = p.Paciente?.NomeCompleto,
            cpf = p.Paciente?.DocumentoCpf,
            modalidade = p.Modalidade.ToString(),
            procedimento = p.DescricaoProcedimento,
            tipoFormulario = p.TipoFormulario,
            segmentoLado = p.SegmentoLado,
            preparoAdequado = p.PreparoAdequado,
            motivoExame = p.MotivoExame,
            medico = $"{p.MedicoSolicitante} ({p.CrmMedico})",
            convenio = p.Convenio,
            sala = p.Sala,
            horario = p.HorarioAgendado,
            status = p.Status
        }));
    }
}

public class ChamarPacienteRequest
{
    public int UsuarioId { get; set; }
}
