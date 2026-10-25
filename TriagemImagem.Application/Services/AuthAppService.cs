using TriagemImagem.Application.DTOs;
using TriagemImagem.Application.Interfaces;
using TriagemImagem.Domain.Entities;
using TriagemImagem.Domain.Repositories;

namespace TriagemImagem.Application.Services;

public class AuthAppService : IAuthAppService
{
    private readonly IUsuarioRepository _usuarioRepository;
    private readonly IAuditoriaRepository _auditoriaRepository;

    public AuthAppService(
        IUsuarioRepository usuarioRepository, 
        IAuditoriaRepository auditoriaRepository)
    {
        _usuarioRepository = usuarioRepository;
        _auditoriaRepository = auditoriaRepository;
    }

    public async Task<UsuarioAutenticadoDto?> AutenticarAsync(LoginRequestDto request)
    {
        if (string.IsNullOrWhiteSpace(request.Login) || string.IsNullOrWhiteSpace(request.Senha))
            return null;

        var usuario = await _usuarioRepository.ObterPorLoginAsync(request.Login);

        if (usuario == null || !usuario.StatusUsuario || usuario.SenhaUsuario != request.Senha)
        {
            await _auditoriaRepository.RegistrarLogAsync(new AuditoriaLog
            {
                UsuarioId = usuario?.Id ?? 1,
                TipoEvento = "LOGIN_FALHA",
                Detalhes = $"Tentativa de login incorreta para o usuário '{request.Login}'."
            });
            return null;
        }

        await _auditoriaRepository.RegistrarLogAsync(new AuditoriaLog
        {
            UsuarioId = usuario.Id,
            TipoEvento = "LOGIN_SUCESSO",
            Detalhes = $"Usuário '{usuario.Login}' ({usuario.Perfil}) autenticado no posto de triagem."
        });

        return new UsuarioAutenticadoDto
        {
            Id = usuario.Id,
            Nome = usuario.Nome,
            Login = usuario.Login,
            Perfil = usuario.Perfil,
            AutenticadoEm = DateTime.UtcNow
        };
    }
}
