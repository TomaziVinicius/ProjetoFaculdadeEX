using TriagemImagem.Domain.Entities;
using TriagemImagem.Domain.Enums;

namespace TriagemImagem.Domain.Repositories;

public interface IExameRepository
{
    Task<Exame?> ObterPorIdAsync(int id);
    Task<IEnumerable<Exame>> ListarDoDiaAsync(DateTime data, ModalidadeExame? modalidade, StatusExame? status);
    Task AtualizarAsync(Exame exame);
}

public interface ITriagemRepository
{
    Task<FormularioTriagem?> ObterPorExameIdAsync(int exameId);
    Task SalvarAsync(FormularioTriagem formulario);
}

public interface IPacienteRepository
{
    Task<Paciente?> ObterPorIdAsync(int id);
    Task<Paciente?> ObterPorCpfAsync(string cpf);
}

public interface IUsuarioRepository
{
    Task<Usuario?> ObterPorLoginAsync(string login);
    Task<Usuario?> ObterPorIdAsync(int id);
}
