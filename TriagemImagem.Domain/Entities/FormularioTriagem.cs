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

public class ItemRespostaTriagem
{
    public int Id { get; set; }
    public int FormularioId { get; set; }
    public FormularioTriagem Formulario { get; set; } = null!;

    public string CodigoPergunta { get; set; } = string.Empty;
    public string PerguntaEnunciado { get; set; } = string.Empty;
    public string RespostaValor { get; set; } = string.Empty; // SIM ou NAO
    public bool Impeditivo { get; set; } = false;
    public string? Justificativa { get; set; }
}

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
