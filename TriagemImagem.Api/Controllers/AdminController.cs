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
