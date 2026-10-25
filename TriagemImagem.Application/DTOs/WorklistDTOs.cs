using TriagemImagem.Domain.Enums;

namespace TriagemImagem.Application.DTOs;

public class ExameWorklistDto
{
    public int Id { get; set; }
    public int Atendimento { get; set; }
    public int PacienteId { get; set; }
    public string PacienteNome { get; set; } = string.Empty;
    public string PacienteCpf { get; set; } = string.Empty;
    public int Idade { get; set; }
    public string Sexo { get; set; } = "M";
    public decimal? PesoKg { get; set; }
    public int? AlturaCm { get; set; }

    public ModalidadeExame Modalidade { get; set; }
    public string ModalidadeDescricao => Modalidade.ToString();
    public string CodigoProcedimento { get; set; } = string.Empty;
    public string DescricaoExame { get; set; } = string.Empty;
    public string Sala { get; set; } = string.Empty;
    public DateTime HorarioAgendado { get; set; }
    public DateTime? HorarioChamada { get; set; }
    public StatusExame Status { get; set; }
    public string StatusDescricao => Status.ToString();

    // Dados resumidos da triagem caso já iniciada ou realizada
    public bool PossuiTriagem { get; set; }
    public StatusRiscoSemaforo? StatusRiscoSemaforo { get; set; }
    public decimal? VolumeContrasteMl { get; set; }
}

public class FiltroWorklistDto
{
    public DateTime? Data { get; set; }
    public ModalidadeExame? Modalidade { get; set; }
    public StatusExame? Status { get; set; }
    public string? TermoBusca { get; set; }
}
