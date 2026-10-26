using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TriagemImagem.Domain.Entities;
using TriagemImagem.Domain.Enums;
using TriagemImagem.Domain.Repositories;
using TriagemImagem.Infrastructure.Data.Context;

namespace TriagemImagem.Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[Produces("application/json")]
public class AdminController : ControllerBase
{
    private readonly IUsuarioRepository _usuarioRepo;
    private readonly IExameRepository _exameRepo;
    private readonly ITriagemRepository _triagemRepo;
    private readonly IAuditoriaRepository _auditoriaRepo;
    private readonly AppDbContext _context;

    public AdminController(
        IUsuarioRepository usuarioRepo,
        IExameRepository exameRepo,
        ITriagemRepository triagemRepo,
        IAuditoriaRepository auditoriaRepo,
        AppDbContext context)
    {
        _usuarioRepo = usuarioRepo;
        _exameRepo = exameRepo;
        _triagemRepo = triagemRepo;
        _auditoriaRepo = auditoriaRepo;
        _context = context;
    }

    /// <summary>
    /// Lista todos os usuários do sistema com seus perfis e status ativo/inativo.
    /// </summary>
    [HttpGet("usuarios")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> ListarUsuarios()
    {
        var usuarios = await _usuarioRepo.ListarTodosAsync();
        return Ok(usuarios.Select(u => new
        {
            u.Id,
            u.Nome,
            u.Login,
            Perfil = u.Perfil.ToString(),
            PerfilId = (int)u.Perfil,
            u.StatusUsuario,
            u.CriadoEm
        }));
    }

    /// <summary>
    /// Cadastra um novo operador ou administrador no sistema.
    /// </summary>
    [HttpPost("usuarios")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CriarUsuario([FromBody] CriarUsuarioRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Nome) || string.IsNullOrWhiteSpace(request.Login))
            return BadRequest(new { success = false, message = "Nome e login são obrigatórios." });

        var existente = await _usuarioRepo.ObterPorLoginAsync(request.Login);
        if (existente != null)
            return BadRequest(new { success = false, message = "Já existe um usuário cadastrado com este login." });

        var usuario = new Usuario
        {
            Nome = request.Nome.Trim(),
            Login = request.Login.Trim().ToLowerInvariant(),
            SenhaUsuario = string.IsNullOrWhiteSpace(request.Senha) ? "123456" : request.Senha,
            Perfil = request.Perfil,
            StatusUsuario = true,
            CriadoEm = DateTime.UtcNow
        };

        await _usuarioRepo.AdicionarAsync(usuario);
        await _usuarioRepo.SalvarAlteracoesAsync();

        await _auditoriaRepo.RegistrarLogAsync(new AuditoriaLog
        {
            UsuarioId = usuario.Id,
            TipoEvento = "USUARIO_CRIADO",
            Detalhes = $"Novo usuário cadastrado pelo administrador: '{usuario.Nome}' ({usuario.Perfil})."
        });

        return CreatedAtAction(nameof(ListarUsuarios), new { id = usuario.Id }, usuario);
    }

    /// <summary>
    /// Alterna o status do operador entre Ativo e Inativo.
    /// </summary>
    [HttpPut("usuarios/{id:int}/toggle-status")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AlternarStatusUsuario(int id)
    {
        var usuario = await _usuarioRepo.ObterPorIdAsync(id);
        if (usuario == null)
            return NotFound(new { success = false, message = "Usuário não encontrado." });

        usuario.StatusUsuario = !usuario.StatusUsuario;
        await _usuarioRepo.AtualizarAsync(usuario);
        await _usuarioRepo.SalvarAlteracoesAsync();

        await _auditoriaRepo.RegistrarLogAsync(new AuditoriaLog
        {
            UsuarioId = id,
            TipoEvento = "USUARIO_STATUS_ALTERADO",
            Detalhes = $"Status do usuário '{usuario.Nome}' alterado para {(usuario.StatusUsuario ? "ATIVO" : "INATIVO")}."
        });

        return Ok(new { success = true, statusUsuario = usuario.StatusUsuario, message = $"Usuário {(usuario.StatusUsuario ? "ativado" : "inativado")} com sucesso." });
    }

    /// <summary>
    /// Redefine a senha de acesso do operador para a senha padrão (123456).
    /// </summary>
    [HttpPut("usuarios/{id:int}/reset-senha")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ResetarSenha(int id)
    {
        var usuario = await _usuarioRepo.ObterPorIdAsync(id);
        if (usuario == null)
            return NotFound(new { success = false, message = "Usuário não encontrado." });

        usuario.SenhaUsuario = "123456";
        await _usuarioRepo.AtualizarAsync(usuario);
        await _usuarioRepo.SalvarAlteracoesAsync();

        await _auditoriaRepo.RegistrarLogAsync(new AuditoriaLog
        {
            UsuarioId = id,
            TipoEvento = "SENHA_RESETADA",
            Detalhes = $"Senha do usuário '{usuario.Login}' redefinida para a senha inicial padrão pelo Administrador."
        });

        return Ok(new { success = true, message = "Senha redefinida com sucesso para '123456'." });
    }

    /// <summary>
    /// Reseta o status de um exame para AGUARDANDO_TRIAGEM para permitir reteste operacional.
    /// </summary>
    [HttpPost("exames/{id:int}/reset-status")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ResetarStatusExame(int id)
    {
        var exame = await _exameRepo.ObterPorIdAsync(id);
        if (exame == null)
            return NotFound(new { success = false, message = "Exame não encontrado." });

        exame.Status = StatusExame.AGUARDANDO_TRIAGEM;
        exame.HorarioChamada = null;
        exame.HorarioExecucao = null;
        await _exameRepo.AtualizarAsync(exame);
        await _exameRepo.SalvarAlteracoesAsync();

        await _auditoriaRepo.RegistrarLogAsync(new AuditoriaLog
        {
            ExameId = id,
            UsuarioId = 1,
            TipoEvento = "EXAME_STATUS_RESETADO",
            Detalhes = $"Administrador resetou o status do exame #{exame.Atendimento} para AGUARDANDO_TRIAGEM para repetição de triagem."
        });

        return Ok(new { success = true, message = $"Exame #{exame.Atendimento} retornado para AGUARDANDO_TRIAGEM." });
    }

    /// <summary>
    /// Altera a sala alocada para um exame de TC ou RM.
    /// </summary>
    [HttpPost("exames/{id:int}/alterar-sala")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AlterarSala(int id, [FromBody] AlterarSalaRequest request)
    {
        var exame = await _exameRepo.ObterPorIdAsync(id);
        if (exame == null)
            return NotFound(new { success = false, message = "Exame não encontrado." });

        var salaAnterior = exame.Sala;
        exame.Sala = request.NovaSala?.Trim().ToUpperInvariant() ?? exame.Sala;
        await _exameRepo.AtualizarAsync(exame);
        await _exameRepo.SalvarAlteracoesAsync();

        await _auditoriaRepo.RegistrarLogAsync(new AuditoriaLog
        {
            ExameId = id,
            UsuarioId = 1,
            TipoEvento = "SALA_ALTERADA",
            Detalhes = $"Sala do exame #{exame.Atendimento} remanejada de '{salaAnterior}' para '{exame.Sala}' pelo Administrador."
        });

        return Ok(new { success = true, novaSala = exame.Sala, message = $"Exame transferido para a sala {exame.Sala}." });
    }

    /// <summary>
    /// Alinha as datas de agendamento de todos os exames para a data atual (Hoje).
    /// Recurso essencial para apresentações acadêmicas e validações sem expiração.
    /// </summary>
    [HttpPost("exames/sincronizar-hoje")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> SincronizarHorariosHoje()
    {
        var exames = await _context.Exames.ToListAsync();
        var hoje = DateTime.Today;

        foreach (var e in exames)
        {
            var hora = e.HorarioAgendado.TimeOfDay;
            e.HorarioAgendado = hoje.Add(hora);
        }

        await _context.SaveChangesAsync();

        await _auditoriaRepo.RegistrarLogAsync(new AuditoriaLog
        {
            UsuarioId = 1,
            TipoEvento = "HORARIOS_SINCRONIZADOS",
            Detalhes = $"Todos os {exames.Count} exames da grade foram alinhados com a data de hoje ({hoje:dd/MM/yyyy})."
        });

        return Ok(new { success = true, totalExamesAtualizados = exames.Count, data = hoje.ToString("yyyy-MM-dd"), message = "Horários dos exames sincronizados para a data de hoje com sucesso." });
    }

    /// <summary>
    /// Atualiza os dados cadastrais e clínicos de um exame na Worklist.
    /// </summary>
    [HttpPut("exames/{id:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> EditarExame(int id, [FromBody] EditarExameRequest request)
    {
        if (id <= 0)
            return BadRequest(new { success = false, message = "O ID do exame deve ser maior que zero." });

        if (string.IsNullOrWhiteSpace(request.PacienteNome))
            return BadRequest(new { success = false, message = "O nome do paciente é obrigatório." });

        if (request.PesoKg <= 0 || request.PesoKg > 400)
            return BadRequest(new { success = false, message = "O peso do paciente deve ser válido (entre 1 e 400 kg)." });

        var exame = await _exameRepo.ObterPorIdAsync(id);
        if (exame == null)
            return NotFound(new { success = false, message = "Exame não encontrado na base de dados." });

        // Atualiza dados do paciente
        if (exame.Paciente != null)
        {
            exame.Paciente.NomeCompleto = request.PacienteNome.Trim();
            if (!string.IsNullOrWhiteSpace(request.DocumentoCpf))
                exame.Paciente.DocumentoCpf = request.DocumentoCpf.Trim();
            exame.Paciente.PesoKg = request.PesoKg;
        }

        // Atualiza dados clínicos do exame
        exame.Modalidade = request.Modalidade;
        if (!string.IsNullOrWhiteSpace(request.Procedimento))
            exame.DescricaoExame = request.Procedimento.Trim();
        if (!string.IsNullOrWhiteSpace(request.SegmentoLado))
            exame.SegmentoLado = request.SegmentoLado.Trim();
        if (!string.IsNullOrWhiteSpace(request.TipoFormulario))
            exame.TipoFormulario = request.TipoFormulario.Trim();
        if (!string.IsNullOrWhiteSpace(request.Sala))
            exame.Sala = request.Sala.Trim().ToUpperInvariant();

        if (!string.IsNullOrWhiteSpace(request.Horario) && TimeSpan.TryParse(request.Horario, out var timeSpan))
        {
            exame.HorarioAgendado = exame.HorarioAgendado.Date.Add(timeSpan);
        }

        await _exameRepo.AtualizarAsync(exame);
        await _exameRepo.SalvarAlteracoesAsync();

        await _auditoriaRepo.RegistrarLogAsync(new AuditoriaLog
        {
            ExameId = id,
            UsuarioId = 1,
            TipoEvento = "EXAME_EDITADO",
            Detalhes = $"Exame #{exame.Atendimento} editado pelo Administrador. Paciente: '{exame.Paciente?.NomeCompleto}', Modalidade: {exame.Modalidade}, Sala: {exame.Sala}."
        });

        return Ok(new { success = true, message = $"Exame #{exame.Atendimento} atualizado com sucesso." });
    }

    /// <summary>
    /// Exclui um exame da Worklist e registros associados.
    /// </summary>
    [HttpDelete("exames/{id:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ExcluirExame(int id)
    {
        if (id <= 0)
            return BadRequest(new { success = false, message = "O ID do exame deve ser maior que zero." });

        var exame = await _exameRepo.ObterPorIdAsync(id);
        if (exame == null)
            return NotFound(new { success = false, message = "Exame não encontrado para exclusão." });

        // Remove triagens e respostas vinculadas se existirem
        var triagem = await _triagemRepo.ObterPorExameIdAsync(id);
        if (triagem != null)
        {
            _context.ItensRespostaTriagem.RemoveRange(triagem.Respostas);
            _context.FormulariosTriagem.Remove(triagem);
        }

        await _exameRepo.RemoverAsync(exame);
        await _exameRepo.SalvarAlteracoesAsync();

        await _auditoriaRepo.RegistrarLogAsync(new AuditoriaLog
        {
            ExameId = id,
            UsuarioId = 1,
            TipoEvento = "EXAME_EXCLUIDO",
            Detalhes = $"Exame #{exame.Atendimento} ({exame.DescricaoExame}) do paciente '{exame.Paciente?.NomeCompleto}' excluído permanentemente pelo Administrador."
        });

        return Ok(new { success = true, message = $"Exame #{exame.Atendimento} excluído com sucesso." });
    }

    /// <summary>
    /// Cadastra um novo agendamento de exame na Worklist para realização de testes clínicos.
    /// </summary>
    [HttpPost("exames")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CriarExame([FromBody] CriarExameRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.PacienteNome))
            return BadRequest(new { success = false, message = "O nome do paciente é obrigatório." });

        if (request.PesoKg <= 0 || request.PesoKg > 400)
            return BadRequest(new { success = false, message = "O peso do paciente deve ser válido (entre 1 e 400 kg)." });

        // Localiza ou cadastra o paciente
        var cpfLimpo = request.DocumentoCpf?.Replace(".", "").Replace("-", "").Trim();
        Paciente? paciente = null;
        if (!string.IsNullOrWhiteSpace(cpfLimpo))
        {
            paciente = await _context.Pacientes.FirstOrDefaultAsync(p => p.DocumentoCpf == cpfLimpo);
        }

        if (paciente == null)
        {
            paciente = new Paciente
            {
                NomeCompleto = request.PacienteNome.Trim(),
                DocumentoCpf = string.IsNullOrWhiteSpace(cpfLimpo) ? $"TESTE{DateTime.UtcNow.Ticks % 100000}" : cpfLimpo,
                PesoKg = request.PesoKg,
                DataNascimento = DateTime.Today.AddYears(-request.Idade > 0 ? -request.Idade : -35),
                Sexo = string.IsNullOrWhiteSpace(request.Sexo) ? "M" : request.Sexo.ToUpperInvariant(),
                CriadoEm = DateTime.UtcNow
            };
            await _context.Pacientes.AddAsync(paciente);
            await _context.SaveChangesAsync();
        }
        else
        {
            paciente.PesoKg = request.PesoKg;
        }

        // Próximo número de atendimento
        var ultimoAtendimento = await _context.Exames.MaxAsync(e => (int?)e.Atendimento) ?? 104525;
        var novoAtendimento = ultimoAtendimento + 1;

        var horario = DateTime.Today.AddHours(10);
        if (!string.IsNullOrWhiteSpace(request.Horario) && TimeSpan.TryParse(request.Horario, out var ts))
        {
            horario = DateTime.Today.Add(ts);
        }

        var exame = new Exame
        {
            PacienteId = paciente.Id,
            Atendimento = novoAtendimento,
            CodigoProcedimento = string.IsNullOrWhiteSpace(request.CodigoProcedimento) ? "41101999" : request.CodigoProcedimento.Trim(),
            SequenciaInterna = 1,
            DescricaoExame = string.IsNullOrWhiteSpace(request.Procedimento) ? "Exame de Diagnóstico por Imagem" : request.Procedimento.Trim(),
            Modalidade = request.Modalidade,
            TipoFormulario = string.IsNullOrWhiteSpace(request.TipoFormulario) ? (request.Modalidade == ModalidadeExame.TC ? "TC_TORAX" : "RM_COLUNA") : request.TipoFormulario.Trim(),
            SegmentoLado = string.IsNullOrWhiteSpace(request.SegmentoLado) ? "Padrão" : request.SegmentoLado.Trim(),
            MotivoExame = string.IsNullOrWhiteSpace(request.MotivoExame) ? "Avaliação clínica de rotina / Teste operacional" : request.MotivoExame.Trim(),
            MedicoSolicitante = "Dr. Plantonista / Médico Solicitante",
            Convenio = string.IsNullOrWhiteSpace(request.Convenio) ? "Particular" : request.Convenio.Trim(),
            Sala = string.IsNullOrWhiteSpace(request.Sala) ? (request.Modalidade == ModalidadeExame.TC ? "TC-01" : "RM-01") : request.Sala.Trim().ToUpperInvariant(),
            HorarioAgendado = horario,
            Status = StatusExame.AGUARDANDO_TRIAGEM,
            CriadoEm = DateTime.UtcNow
        };

        await _exameRepo.AdicionarAsync(exame);
        await _exameRepo.SalvarAlteracoesAsync();

        await _auditoriaRepo.RegistrarLogAsync(new AuditoriaLog
        {
            ExameId = exame.Id,
            UsuarioId = 1,
            TipoEvento = "EXAME_CRIADO",
            Detalhes = $"Novo agendamento criado: Atendimento #{exame.Atendimento} para o paciente '{paciente.NomeCompleto}' ({exame.Modalidade} - {exame.DescricaoExame}) na sala {exame.Sala}."
        });

        return CreatedAtAction(nameof(ListarUsuarios), new { id = exame.Id }, new
        {
            success = true,
            message = $"Exame #{exame.Atendimento} cadastrado com sucesso!",
            exame = new
            {
                exame.Id,
                exame.Atendimento,
                pacienteNome = paciente.NomeCompleto,
                documentoCpf = paciente.DocumentoCpf,
                pesoKg = paciente.PesoKg,
                modalidade = exame.Modalidade.ToString(),
                procedimento = exame.DescricaoExame,
                tipoFormulario = exame.TipoFormulario,
                segmentoLado = exame.SegmentoLado,
                sala = exame.Sala,
                horario = exame.HorarioAgendado.ToString("HH:mm"),
                status = "AGUARDANDO_TRIAGEM"
            }
        });
    }
}

public class CriarExameRequest
{
    public string PacienteNome { get; set; } = string.Empty;
    public string? DocumentoCpf { get; set; }
    public int Idade { get; set; } = 35;
    public string? Sexo { get; set; } = "M";
    public decimal PesoKg { get; set; } = 70;
    public ModalidadeExame Modalidade { get; set; } = ModalidadeExame.RM;
    public string Procedimento { get; set; } = string.Empty;
    public string? CodigoProcedimento { get; set; }
    public string? SegmentoLado { get; set; }
    public string? TipoFormulario { get; set; }
    public string? MotivoExame { get; set; }
    public string? Convenio { get; set; }
    public string? Sala { get; set; }
    public string? Horario { get; set; }
}

public class EditarExameRequest
{
    public string PacienteNome { get; set; } = string.Empty;
    public string? DocumentoCpf { get; set; }
    public decimal PesoKg { get; set; }
    public ModalidadeExame Modalidade { get; set; }
    public string Procedimento { get; set; } = string.Empty;
    public string? SegmentoLado { get; set; }
    public string? TipoFormulario { get; set; }
    public string? Sala { get; set; }
    public string? Horario { get; set; }
}

public class CriarUsuarioRequest
{
    public string Nome { get; set; } = string.Empty;
    public string Login { get; set; } = string.Empty;
    public string? Senha { get; set; } = "123456";
    public PerfilUsuario Perfil { get; set; } = PerfilUsuario.ESTAGIARIO;
}

public class AlterarSalaRequest
{
    public string NovaSala { get; set; } = string.Empty;
}
