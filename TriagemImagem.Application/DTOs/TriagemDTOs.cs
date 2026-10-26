using TriagemImagem.Domain.Enums;

namespace TriagemImagem.Application.DTOs;

public class ItemRespostaDto
{
    public string CodigoPergunta { get; set; } = string.Empty;
    public string PerguntaEnunciado { get; set; } = string.Empty;
    public string RespostaValor { get; set; } = "NAO"; // SIM ou NAO
    public bool Impeditivo { get; set; }
    public string? Justificativa { get; set; }
}

public class SalvarTriagemRequestDto
{
    public int ExameId { get; set; }
    public int UsuarioResponsavelId { get; set; }
    public decimal? PesoKg { get; set; }
    public decimal? VolumeContrasteMl { get; set; }
    public string? LoteContraste { get; set; }
    public string? ObservacoesClinicas { get; set; }
    public bool? ConcluirELiberar { get; set; }
    public List<ItemRespostaDto> Respostas { get; set; } = new();
}

public class DetalhesTriagemDto
{
    public int Id { get; set; }
    public int ExameId { get; set; }
    public int UsuarioResponsavelId { get; set; }
    public string UsuarioResponsavelNome { get; set; } = string.Empty;

    public StatusRiscoSemaforo StatusRisco { get; set; }
    public string StatusRiscoDescricao => StatusRisco.ToString();
    public bool BloqueiaExame => StatusRisco == StatusRiscoSemaforo.VERMELHO;

    public decimal? VolumeContrasteMl { get; set; }
    public string? LoteContraste { get; set; }
    public string? ObservacoesClinicas { get; set; }

    public DateTime IniciadoEm { get; set; }
    public DateTime? FinalizadoEm { get; set; }
    public string? CaminhoArquivoArmazenado { get; set; }

    public List<ItemRespostaDto> Respostas { get; set; } = new();
    public List<string> MotivosContraindicacao { get; set; } = new();
    public List<string> AlertasAtencao { get; set; } = new();
}
