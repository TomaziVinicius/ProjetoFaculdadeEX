namespace TriagemImagem.Application.DTOs;

public class AuditoriaLogDto
{
    public int Id { get; set; }
    public int UsuarioId { get; set; }
    public string UsuarioNome { get; set; } = string.Empty;
    public int? ExameId { get; set; }
    public string TipoEvento { get; set; } = string.Empty;
    public string Detalhes { get; set; } = string.Empty;
    public string? IpOrigem { get; set; }
    public DateTime CriadoEm { get; set; }
}
