using TriagemImagem.Domain.Entities;

namespace TriagemImagem.Domain.Repositories;

/// <summary>
/// Contrato de persistência para Pacientes.
/// </summary>
public interface IPacienteRepository
{
    Task<Paciente?> ObterPorIdAsync(int id);
    Task<Paciente?> ObterPorCpfAsync(string cpf);
    Task<IEnumerable<Paciente>> BuscarPorNomeAsync(string nome);
    Task AdicionarAsync(Paciente paciente);
    Task AtualizarAsync(Paciente paciente);
    Task<int> SalvarAlteracoesAsync();
}
