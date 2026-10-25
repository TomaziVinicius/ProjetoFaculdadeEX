namespace TriagemImagem.Domain.Entities;

public class AuditoriaLog
{
    public int Id { get; set; }
    public int UsuarioId { get; set; }
    public Usuario Usuario { get; set; } = null!;

    public int? ExameId { get; set; }
    public string TipoEvento { get; set; } = string.Empty;
    public string Detalhes { get; set; } = string.Empty;
    public string? IpOrigem { get; set; }
    public DateTime CriadoEm { get; set; } = DateTime.UtcNow;
}
