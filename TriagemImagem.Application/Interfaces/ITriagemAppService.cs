using TriagemImagem.Application.DTOs;

namespace TriagemImagem.Application.Interfaces;

public interface ITriagemAppService
{
    Task<DetalhesTriagemDto> SalvarTriagemAsync(SalvarTriagemRequestDto request);
    Task<DetalhesTriagemDto?> ObterPorExameIdAsync(int exameId);
    Task<bool> LiberarExameAsync(int exameId, int usuarioId, string? observacao = null);
    Task<bool> CancelarExameAsync(int exameId, int usuarioId, string motivo);
}
