using TriagemImagem.Domain.Enums;

namespace TriagemImagem.Application.DTOs;

public class CalcularDoseRequestDto
{
    public ModalidadeExame Modalidade { get; set; } = ModalidadeExame.TC;
    public decimal PesoKg { get; set; }
    public decimal? FatorMlPorKg { get; set; }
}

public class ResultadoDoseDto
{
    public decimal PesoKg { get; set; }
    public ModalidadeExame Modalidade { get; set; }
    public string ModalidadeDescricao => Modalidade.ToString();
    public decimal FatorMlPorKg { get; set; }
    public decimal VolumeCalculadoMl { get; set; }
    public decimal VolumeAjustadoMl { get; set; }
    public bool TetoMaximoAtingido { get; set; }
    public string? AlertaClinico { get; set; }
}
