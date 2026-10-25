using TriagemImagem.Domain.Entities;
using TriagemImagem.Domain.Enums;

namespace TriagemImagem.Domain.Services;

/// <summary>
/// Contrato do serviço de domínio responsável por aplicar as regras clínicas do Checklist de Aviação
/// e determinar o Semáforo Clínico (VERDE, AMARELO, VERMELHO) para TC e RM.
/// </summary>
public interface ISemaforoClinicoDomainService
{
    /// <summary>
    /// Avalia um conjunto de respostas da triagem e retorna o nível de risco e bloqueios clínicos.
    /// </summary>
    ResultadoSemaforoClinico Avaliar(
        ModalidadeExame modalidade, 
        IEnumerable<ItemRespostaTriagem> respostas, 
        decimal? pesoKg = null);
}
