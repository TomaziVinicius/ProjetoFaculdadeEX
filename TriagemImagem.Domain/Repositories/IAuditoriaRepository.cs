using TriagemImagem.Domain.Entities;

namespace TriagemImagem.Domain.Repositories;

/// <summary>
/// Contrato de persistência para Trilha de Auditoria (Logs de conformidade e segurança).
/// </summary>
public interface IAuditoriaRepository
{
    Task RegistrarLogAsync(AuditoriaLog log);
    Task<IEnumerable<AuditoriaLog>> ListarUltimosLogsAsync(int limite = 50);
    Task<IEnumerable<AuditoriaLog>> ListarPorExameAsync(int exameId);
    Task<int> SalvarAlteracoesAsync();
}
