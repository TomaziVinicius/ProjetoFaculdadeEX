namespace TriagemImagem.Application.DTOs;

public class AtendimentoDTO
{
    public int Id { get; set; }
    public int Atendimento { get; set; }
    public string PacienteNome { get; set; } = string.Empty;
    public string DocumentoCpf { get; set; } = string.Empty;
    public int Idade { get; set; }
    public string Modalidade { get; set; } = string.Empty;
    public string DescricaoExame { get; set; } = string.Empty;
    public string Sala { get; set; } = string.Empty;
    public DateTime HorarioAgendado { get; set; }
    public string Status { get; set; } = string.Empty;
}

public class TriagemInputDTO
{
    public int AtendimentoId { get; set; }
    public decimal? PesoKg { get; set; }
    public decimal? VolumeContrasteMl { get; set; }
    public string? LoteContraste { get; set; }
    public string? Observacoes { get; set; }
    public Dictionary<string, string> Respostas { get; set; } = new();
}

public class LiberacaoExameDTO
{
    public int FormularioId { get; set; }
    public bool Confirmado { get; set; }
    public string? Observacao { get; set; }
}
