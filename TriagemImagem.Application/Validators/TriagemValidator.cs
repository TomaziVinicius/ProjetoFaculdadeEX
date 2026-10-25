using TriagemImagem.Application.DTOs;
using TriagemImagem.Domain.Exceptions;

namespace TriagemImagem.Application.Validators;

public static class TriagemValidator
{
    public static void Validar(SalvarTriagemRequestDto request)
    {
        if (request == null)
            throw new DomainException("A requisição de triagem não pode ser nula.");

        if (request.ExameId <= 0)
            throw new DomainException("O identificador do exame (ExameId) é obrigatório.");

        if (request.UsuarioResponsavelId <= 0)
            throw new DomainException("O usuário responsável pela triagem deve ser informado.");

        if (request.Respostas == null || !request.Respostas.Any())
            throw new DomainException("O checklist de triagem deve conter ao menos uma pergunta respondida.");

        if (request.PesoKg.HasValue && (request.PesoKg.Value <= 0 || request.PesoKg.Value > 400))
            throw new DomainException($"O peso informado ({request.PesoKg.Value} kg) é inválido. Deve ser entre 0.1 e 400 kg.");

        if (request.VolumeContrasteMl.HasValue && request.VolumeContrasteMl.Value < 0)
            throw new DomainException("O volume de contraste não pode ser negativo.");

        foreach (var item in request.Respostas)
        {
            if (string.IsNullOrWhiteSpace(item.CodigoPergunta))
                throw new DomainException("O código identificador da pergunta do checklist é obrigatório.");

            var resposta = item.RespostaValor?.Trim().ToUpperInvariant();
            if (resposta != "SIM" && resposta != "NAO")
                throw new DomainException($"Resposta inválida para o item '{item.CodigoPergunta}'. Deve ser 'SIM' ou 'NAO'.");

            if (item.Impeditivo && resposta == "SIM" && string.IsNullOrWhiteSpace(item.Justificativa))
            {
                // Alerta ou justificativa opcional, mas boa prática
            }
        }
    }
}
