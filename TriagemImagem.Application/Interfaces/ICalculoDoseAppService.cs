using TriagemImagem.Application.DTOs;

namespace TriagemImagem.Application.Interfaces;

public interface ICalculoDoseAppService
{
    ResultadoDoseDto CalcularDose(CalcularDoseRequestDto request);
}
