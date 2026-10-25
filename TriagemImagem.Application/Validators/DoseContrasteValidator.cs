using TriagemImagem.Application.DTOs;
using TriagemImagem.Domain.Exceptions;

namespace TriagemImagem.Application.Validators;

public static class DoseContrasteValidator
{
    public static void Validar(CalcularDoseRequestDto request)
    {
        if (request == null)
            throw new DomainException("A requisição de cálculo de dose não pode ser nula.");

        if (request.PesoKg <= 0 || request.PesoKg > 400)
            throw new DomainException($"Peso corporal inválido: {request.PesoKg} kg. O peso deve estar entre 0.1 e 400 kg.");

        if (request.FatorMlPorKg.HasValue && (request.FatorMlPorKg.Value <= 0 || request.FatorMlPorKg.Value > 5.0m))
            throw new DomainException($"Fator de proporção inválido: {request.FatorMlPorKg.Value} mL/kg. Deve estar entre 0.01 e 5.0 mL/kg.");
    }
}
