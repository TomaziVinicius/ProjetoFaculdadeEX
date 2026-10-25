using TriagemImagem.Domain.Enums;

namespace TriagemImagem.Domain.Entities;

public class FormularioTriagem
{
    public int Id { get; set; }
    public int ExameId { get; set; }
    public Exame Exame { get; set; } = null!;

    public int UsuarioResponsavelId { get; set; }
    public Usuario UsuarioResponsavel { get; set; } = null!;

    public decimal? VolumeContrasteMl { get; set; }
    public string? LoteContraste { get; set; }
    public string? CaminhoArquivoArmazenado { get; set; }

    public StatusRiscoSemaforo StatusRisco { get; set; } = StatusRiscoSemaforo.VERDE;
    public string? ObservacoesClinicas { get; set; }

    public DateTime IniciadoEm { get; set; } = DateTime.UtcNow;
    public DateTime? FinalizadoEm { get; set; }

    public ICollection<ItemRespostaTriagem> Respostas { get; set; } = new List<ItemRespostaTriagem>();
}
