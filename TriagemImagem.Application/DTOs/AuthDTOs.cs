using TriagemImagem.Domain.Enums;

namespace TriagemImagem.Application.DTOs;

public class LoginRequestDto
{
    public string Login { get; set; } = string.Empty;
    public string Senha { get; set; } = string.Empty;
}

public class UsuarioAutenticadoDto
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Login { get; set; } = string.Empty;
    public PerfilUsuario Perfil { get; set; }
    public string PerfilDescricao => Perfil.ToString();
    public DateTime AutenticadoEm { get; set; } = DateTime.UtcNow;
}
