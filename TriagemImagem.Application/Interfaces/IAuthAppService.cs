using TriagemImagem.Application.DTOs;

namespace TriagemImagem.Application.Interfaces;

public interface IAuthAppService
{
    Task<UsuarioAutenticadoDto?> AutenticarAsync(LoginRequestDto request);
}
