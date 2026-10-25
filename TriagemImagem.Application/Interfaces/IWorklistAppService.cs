using TriagemImagem.Application.DTOs;

namespace TriagemImagem.Application.Interfaces;

public interface IWorklistAppService
{
    Task<IEnumerable<ExameWorklistDto>> ObterWorklistAsync(FiltroWorklistDto filtro);
    Task<ExameWorklistDto?> ObterExamePorIdAsync(int exameId);
    Task<bool> ChamarPacienteAsync(int exameId, int usuarioId);
}
