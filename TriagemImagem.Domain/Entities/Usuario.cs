using TriagemImagem.Domain.Enums;

namespace TriagemImagem.Domain.Entities;

public class Usuario
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Login { get; set; } = string.Empty;
    public string SenhaUsuario { get; set; } = string.Empty;
    public PerfilUsuario Perfil { get; set; } = PerfilUsuario.TECNICO;
    public bool StatusUsuario { get; set; } = true;
    public DateTime CriadoEm { get; set; } = DateTime.UtcNow;
}
