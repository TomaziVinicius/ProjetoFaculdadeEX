using TriagemImagem.Domain.Enums;

namespace TriagemImagem.Domain.Entities;

public class Exame
{
    public int Id { get; set; }
    public int PacienteId { get; set; }
    public Paciente Paciente { get; set; } = null!;

    public int Atendimento { get; set; }
    public string CodigoProcedimento { get; set; } = string.Empty;
    public int SequenciaInterna { get; set; } = 1;
    public string DescricaoExame { get; set; } = string.Empty;
    public ModalidadeExame Modalidade { get; set; } = ModalidadeExame.TC;
    public string Sala { get; set; } = string.Empty;

    public DateTime HorarioAgendado { get; set; }
    public DateTime? HorarioChamada { get; set; }
    public DateTime? HorarioExecucao { get; set; }

    public StatusExame Status { get; set; } = StatusExame.AGUARDANDO_TRIAGEM;
    public DateTime CriadoEm { get; set; } = DateTime.UtcNow;
}
