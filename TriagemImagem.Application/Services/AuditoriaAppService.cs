using TriagemImagem.Application.DTOs;
using TriagemImagem.Application.Interfaces;
using TriagemImagem.Domain.Repositories;

namespace TriagemImagem.Application.Services;

public class AuditoriaAppService : IAuditoriaAppService
{
    private readonly IAuditoriaRepository _auditoriaRepository;

    public AuditoriaAppService(IAuditoriaRepository auditoriaRepository)
    {
        _auditoriaRepository = auditoriaRepository;
    }

    public async Task<IEnumerable<AuditoriaLogDto>> ObterUltimosLogsAsync(int limite = 50)
    {
        var logs = await _auditoriaRepository.ListarUltimosLogsAsync(limite);
        return logs.Select(l => new AuditoriaLogDto
        {
            Id = l.Id,
            UsuarioId = l.UsuarioId,
            UsuarioNome = l.Usuario?.Nome ?? $"Usuário #{l.UsuarioId}",
            ExameId = l.ExameId,
            TipoEvento = l.TipoEvento,
            Detalhes = l.Detalhes,
            IpOrigem = l.IpOrigem,
            CriadoEm = l.CriadoEm
        });
    }
}
