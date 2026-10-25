using TriagemImagem.Application.DTOs;
using TriagemImagem.Application.Interfaces;
using TriagemImagem.Application.Validators;
using TriagemImagem.Domain.Enums;
using TriagemImagem.Domain.ValueObjects;

namespace TriagemImagem.Application.Services;

public class CalculoDoseAppService : ICalculoDoseAppService
{
    public ResultadoDoseDto CalcularDose(CalcularDoseRequestDto request)
    {
        DoseContrasteValidator.Validar(request);

        DoseContraste dose;
        if (request.Modalidade == ModalidadeExame.TC)
        {
            var fator = request.FatorMlPorKg ?? 1.5m;
            dose = DoseContraste.CalcularParaTC(request.PesoKg, fator);
        }
        else
        {
            var fator = request.FatorMlPorKg ?? 0.2m;
            dose = DoseContraste.CalcularParaRM(request.PesoKg, fator);
        }

        return new ResultadoDoseDto
        {
            PesoKg = dose.PesoKg,
            Modalidade = dose.Modalidade,
            FatorMlPorKg = dose.FatorMlPorKg,
            VolumeCalculadoMl = dose.VolumeCalculadoMl,
            VolumeAjustadoMl = dose.VolumeAjustadoMl,
            TetoMaximoAtingido = dose.TetoMaximoAtingido,
            AlertaClinico = dose.AlertaClinico
        };
    }
}
