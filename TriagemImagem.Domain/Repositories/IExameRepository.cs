using TriagemImagem.Domain.Entities;
using TriagemImagem.Domain.Enums;

namespace TriagemImagem.Domain.Repositories;

/// <summary>
/// Contrato de persistência para Exames agendados e Worklist da estação de trabalho.
/// </summary>
public interface IExameRepository
{
    Task<Exame?> ObterPorIdAsync(int id);
    Task<IEnumerable<Exame>> ListarDoDiaAsync(DateTime data, ModalidadeExame? modalidade = null, StatusExame? status = null);
    Task<IEnumerable<Exame>> ListarTodosAsync(ModalidadeExame? modalidade = null, StatusExame? status = null);
    Task<IEnumerable<Exame>> BuscarAsync(string? termoBusca, DateTime? data = null);
    Task AdicionarAsync(Exame exame);
    Task AtualizarAsync(Exame exame);
    Task<int> SalvarAlteracoesAsync();
}
