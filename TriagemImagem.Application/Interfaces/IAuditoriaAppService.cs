using TriagemImagem.Application.DTOs;

namespace TriagemImagem.Application.Interfaces;

public interface IAuditoriaAppService
{
    Task<IEnumerable<AuditoriaLogDto>> ObterUltimosLogsAsync(int limite = 50);
}
