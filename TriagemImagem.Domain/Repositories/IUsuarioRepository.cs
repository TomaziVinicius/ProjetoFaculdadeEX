using TriagemImagem.Domain.Entities;

namespace TriagemImagem.Domain.Repositories;

/// <summary>
/// Contrato de persistência para Usuários e autenticação/perfis do sistema.
/// </summary>
public interface IUsuarioRepository
{
    Task<Usuario?> ObterPorIdAsync(int id);
    Task<Usuario?> ObterPorLoginAsync(string login);
    Task<IEnumerable<Usuario>> ListarAtivosAsync();
    Task<IEnumerable<Usuario>> ListarTodosAsync();
    Task AdicionarAsync(Usuario usuario);
    Task AtualizarAsync(Usuario usuario);
    Task<int> SalvarAlteracoesAsync();
}
