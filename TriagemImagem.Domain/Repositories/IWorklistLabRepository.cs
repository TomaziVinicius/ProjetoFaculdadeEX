using TriagemImagem.Domain.Entities;

namespace TriagemImagem.Domain.Repositories;

/// <summary>
/// Contrato de acesso aos dados do Banco de Dados 2 (Worklist do Laboratório / HIS-RIS)
/// </summary>
public interface IWorklistLabRepository
{
    Task<IEnumerable<WorklistPedidoLab>> ListarPedidosAsync();
    Task<WorklistPedidoLab?> ObterPorAtendimentoAsync(int atendimento);
    Task AtualizarStatusAsync(int atendimento, string status);
}
