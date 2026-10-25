using TriagemImagem.Domain.Entities;
using TriagemImagem.Domain.Enums;

namespace TriagemImagem.Domain.Services;

/// <summary>
/// Implementação das regras clínicas do Checklist de Aviação para Tomografia (TC) e Ressonância Magnética (RM).
/// Define o nível de Semáforo Clínico (VERDE, AMARELO, VERMELHO) e motivos de contraindicação ou precaução.
/// </summary>
public class SemaforoClinicoDomainService : ISemaforoClinicoDomainService
{
    // Códigos de perguntas com contraindicação absoluta em Ressonância Magnética (RM)
    private static readonly HashSet<string> PerguntasImpeditivasRM = new(StringComparer.OrdinalIgnoreCase)
    {
        "RM_MARCAPASSO",
        "RM_CLIPE_ANEURISMA",
        "RM_IMPLANTE_COCLEAR",
        "RM_FRAGMENTO_METAL"
    };

    // Códigos de perguntas com contraindicação crítica em Tomografia (TC)
    private static readonly HashSet<string> PerguntasImpeditivasTC = new(StringComparer.OrdinalIgnoreCase)
    {
        "TC_ALERGIA_IODO",
        "TC_INSUF_RENAL"
    };

    public ResultadoSemaforoClinico Avaliar(
        ModalidadeExame modalidade, 
        IEnumerable<ItemRespostaTriagem> respostas, 
        decimal? pesoKg = null)
    {
        var listaRespostas = respostas?.ToList() ?? new List<ItemRespostaTriagem>();
        var motivosContraindicacao = new List<string>();
        var alertasAtencao = new List<string>();

        // 1. Verificação de peso para cálculo/segurança clínica
        if (pesoKg.HasValue && pesoKg.Value <= 0)
        {
            alertasAtencao.Add("Peso informado inválido ou zerado. Necessário pesar o paciente para cálculo de contraste.");
        }

        // 2. Avaliação de cada resposta do checklist
        foreach (var item in listaRespostas)
        {
            var codigo = item.CodigoPergunta?.Trim() ?? string.Empty;
            var resposta = item.RespostaValor?.Trim().ToUpperInvariant() ?? "NAO";
            var isSim = resposta == "SIM";

            // Se o item já foi marcado explicitamente como impeditivo na resposta
            if (item.Impeditivo && isSim)
            {
                var detalhe = !string.IsNullOrWhiteSpace(item.Justificativa) 
                    ? $" ({item.Justificativa})" 
                    : string.Empty;
                motivosContraindicacao.Add($"[IMPEDIMENTO CLÍNICO] {item.PerguntaEnunciado}{detalhe}");
                continue;
            }

            // Regras específicas para Ressonância Magnética
            if (modalidade == ModalidadeExame.RM)
            {
                if (isSim && PerguntasImpeditivasRM.Contains(codigo))
                {
                    item.Impeditivo = true;
                    motivosContraindicacao.Add($"[RM CONTRAINDICADA] {item.PerguntaEnunciado} (Risco ferromagnético grave / deslocamento ou aquecimento por campo magnético)");
                }
                else if (isSim)
                {
                    if (codigo.Equals("RM_CLAUSTROFOBIA", StringComparison.OrdinalIgnoreCase))
                    {
                        alertasAtencao.Add("Paciente relata claustrofobia: avaliar protocolo de acolhimento, iluminação do túnel ou necessidade de sedação leve.");
                    }
                    else if (codigo.Equals("RM_TATUAGEM_RECENTE", StringComparison.OrdinalIgnoreCase))
                    {
                        alertasAtencao.Add("Tatuagem recente ou maquiagem definitiva: monitorar sensação de queimação no local durante o exame.");
                    }
                    else if (codigo.Equals("RM_PROTESE_METALLICA", StringComparison.OrdinalIgnoreCase))
                    {
                        alertasAtencao.Add("Prótese metálica presente: verificar documentação do fabricante para segurança em campo de 1.5T/3.0T.");
                    }
                }
            }

            // Regras específicas para Tomografia Computadorizada
            if (modalidade == ModalidadeExame.TC)
            {
                if (isSim && PerguntasImpeditivasTC.Contains(codigo))
                {
                    // Alergia prévia ou insuficiência renal sem avaliação laboratorial
                    if (codigo.Equals("TC_ALERGIA_IODO", StringComparison.OrdinalIgnoreCase))
                    {
                        item.Impeditivo = true;
                        motivosContraindicacao.Add("[TC CONTRAINDICADA] Histórico afirmativo de reação adversa / anafilactoide prévia ao meio de contraste iodado. Exige protocolo de pré-medicação ou avaliação médica.");
                    }
                    else if (codigo.Equals("TC_INSUF_RENAL", StringComparison.OrdinalIgnoreCase))
                    {
                        item.Impeditivo = true;
                        motivosContraindicacao.Add("[TC CONTRAINDICADA] Insuficiência renal declarada. Obrigatório conferir dosagem recente de Creatinina e taxa de filtração glomerular (eGFR) antes de injetar contraste.");
                    }
                }
                else if (isSim)
                {
                    if (codigo.Equals("TC_DIABETES_METFORMINA", StringComparison.OrdinalIgnoreCase))
                    {
                        alertasAtencao.Add("Uso de Metformina: orientar o paciente sobre protocolo de suspensão da medicação por 48 horas após injeção de contraste iodado conforme diretriz.");
                    }
                    else if (codigo.Equals("TC_ASMA_BRONQUITE", StringComparison.OrdinalIgnoreCase))
                    {
                        alertasAtencao.Add("Asma / Bronquite espástica: paciente com hiperreatividade brônquica. Manter suporte de oxigenioterapia e broncodilatador de prontidão.");
                    }
                    else if (codigo.Equals("TC_GESTANTE", StringComparison.OrdinalIgnoreCase))
                    {
                        alertasAtencao.Add("Suspeita de gestação: aplicar princípio ALARA e verificar justificativa clínica formal pelo médico radiologista.");
                    }
                }

                // Regra de jejum para contraste: se Jejum NÃO foi cumprido
                if (codigo.Equals("TC_JEJUM_4H", StringComparison.OrdinalIgnoreCase) && resposta == "NAO")
                {
                    alertasAtencao.Add("Jejum mínimo de 4 horas não realizado: risco elevado de broncoaspiração em caso de náusea induzida por contraste.");
                }
            }
        }

        // 3. Determinação final do Semáforo Clínico
        if (motivosContraindicacao.Any())
        {
            return ResultadoSemaforoClinico.Contraindicado(motivosContraindicacao, alertasAtencao);
        }

        if (alertasAtencao.Any())
        {
            return ResultadoSemaforoClinico.Atencao(alertasAtencao);
        }

        return ResultadoSemaforoClinico.Liberado();
    }
}
