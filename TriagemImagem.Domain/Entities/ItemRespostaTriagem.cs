namespace TriagemImagem.Domain.Entities;

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
