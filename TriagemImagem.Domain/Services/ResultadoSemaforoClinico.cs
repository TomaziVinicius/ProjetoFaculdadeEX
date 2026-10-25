using TriagemImagem.Domain.Enums;

namespace TriagemImagem.Domain.Services;

/// <summary>
/// Objeto de resultado da avaliação do Semáforo Clínico de Triagem.
/// </summary>
public sealed class ResultadoSemaforoClinico
{
    public StatusRiscoSemaforo StatusRisco { get; }
    public bool BloqueiaExame => StatusRisco == StatusRiscoSemaforo.VERMELHO;
    public IReadOnlyList<string> MotivosContraindicacao { get; }
    public IReadOnlyList<string> AlertasAtencao { get; }
    public DateTime AvaliadoEm { get; }

    public bool IsVerde => StatusRisco == StatusRiscoSemaforo.VERDE;
    public bool IsAmarelo => StatusRisco == StatusRiscoSemaforo.AMARELO;
    public bool IsVermelho => StatusRisco == StatusRiscoSemaforo.VERMELHO;

    public ResultadoSemaforoClinico(
        StatusRiscoSemaforo statusRisco,
        IEnumerable<string>? motivosContraindicacao = null,
        IEnumerable<string>? alertasAtencao = null)
    {
        StatusRisco = statusRisco;
        MotivosContraindicacao = (motivosContraindicacao ?? Enumerable.Empty<string>()).ToList().AsReadOnly();
        AlertasAtencao = (alertasAtencao ?? Enumerable.Empty<string>()).ToList().AsReadOnly();
        AvaliadoEm = DateTime.UtcNow;
    }

    public static ResultadoSemaforoClinico Liberado() =>
        new(StatusRiscoSemaforo.VERDE);

    public static ResultadoSemaforoClinico Atencao(IEnumerable<string> alertas) =>
        new(StatusRiscoSemaforo.AMARELO, alertasAtencao: alertas);

    public static ResultadoSemaforoClinico Contraindicado(IEnumerable<string> motivos, IEnumerable<string>? alertas = null) =>
        new(StatusRiscoSemaforo.VERMELHO, motivosContraindicacao: motivos, alertasAtencao: alertas);
}
