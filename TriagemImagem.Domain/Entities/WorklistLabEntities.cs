using TriagemImagem.Domain.Enums;

namespace TriagemImagem.Domain.Entities;

/// <summary>
/// Representa o cadastro de paciente residente no Banco de Dados 2 (HIS / Worklist do Laboratório)
/// </summary>
public class WorklistPacienteLab
{
    public int Id { get; set; }
    public string DocumentoCpf { get; set; } = string.Empty;
    public string NomeCompleto { get; set; } = string.Empty;
    public DateTime DataNascimento { get; set; }
    public string Sexo { get; set; } = "M";
    public decimal? PesoKg { get; set; }
    public int? AlturaCm { get; set; }
    public string? Telefone { get; set; }
    public DateTime CriadoEm { get; set; } = DateTime.UtcNow;

    public ICollection<WorklistPedidoLab> Pedidos { get; set; } = new List<WorklistPedidoLab>();
}

/// <summary>
/// Representa o pedido de exame agendado residente no Banco de Dados 2 (HIS / Worklist do Laboratório)
/// </summary>
public class WorklistPedidoLab
{
    public int Id { get; set; }
    public int PacienteId { get; set; }
    public WorklistPacienteLab Paciente { get; set; } = null!;

    public int Atendimento { get; set; }
    public ModalidadeExame Modalidade { get; set; }
    public string CodigoProcedimento { get; set; } = string.Empty;
    public string DescricaoProcedimento { get; set; } = string.Empty;

    /// <summary>
    /// Identifica o formulário específico para o exame:
    /// 'RM_COLUNA', 'RM_BRACO_ANTEBRACO', 'RM_BACIA_QUADRIL', 'TC_TORAX', 'TC_ABDOMEN'
    /// </summary>
    public string TipoFormulario { get; set; } = "RM_COLUNA";

    /// <summary>
    /// Segmento ou lado: 'Cervical', 'Torácica', 'Lombar', 'Direito', 'Esquerdo', 'Bilateral'
    /// </summary>
    public string? SegmentoLado { get; set; }

    public bool PreparoAdequado { get; set; } = true;
    public string? PreparoMotivoInadequado { get; set; }
    public string? MotivoExame { get; set; }

    public string MedicoSolicitante { get; set; } = string.Empty;
    public string CrmMedico { get; set; } = string.Empty;
    public string Convenio { get; set; } = "Unimed";

    public string Sala { get; set; } = string.Empty;
    public DateTime HorarioAgendado { get; set; }
    public DateTime? HorarioChamada { get; set; }

    public string Status { get; set; } = "AGENDADO";
    public DateTime CriadoEm { get; set; } = DateTime.UtcNow;
}
