using TriagemImagem.Domain.Entities;

namespace TriagemImagem.Domain.Repositories;

/// <summary>
/// Contrato de persistência para Formulários de Triagem e Respostas do Checklist.
/// </summary>
public interface ITriagemRepository
{
    Task<FormularioTriagem?> ObterPorIdAsync(int id);
    Task<FormularioTriagem?> ObterPorExameIdAsync(int exameId);
    Task SalvarAsync(FormularioTriagem formulario);
    Task AtualizarAsync(FormularioTriagem formulario);
    Task<int> SalvarAlteracoesAsync();
}
