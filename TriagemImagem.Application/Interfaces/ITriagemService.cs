using TriagemImagem.Application.DTOs;

namespace TriagemImagem.Application.Interfaces;

public interface ITriagemService
{
    Task<IEnumerable<AtendimentoDTO>> ListarAtendimentosDoDiaAsync();
    Task<bool> SalvarTriagemAsync(TriagemInputDTO dto, int usuarioId);
    Task<bool> LiberarExameAsync(LiberacaoExameDTO dto, int usuarioId);
}
