using TriagemImagem.Application.DTOs;
using TriagemImagem.Application.Services;
using TriagemImagem.Domain.Entities;
using TriagemImagem.Domain.Enums;
using TriagemImagem.Domain.Exceptions;
using TriagemImagem.Domain.Repositories;
using TriagemImagem.Domain.Services;
using Xunit;

namespace TriagemImagem.Tests.Application;

public class TriagemAppServiceTests
{
    private class FakeExameRepository : IExameRepository
    {
        public Exame? ExameArmazenado { get; set; }

        public Task<Exame?> ObterPorIdAsync(int id) => Task.FromResult(ExameArmazenado);
        public Task<IEnumerable<Exame>> ListarDoDiaAsync(DateTime data, ModalidadeExame? modalidade, StatusExame? status) =>
            Task.FromResult<IEnumerable<Exame>>(ExameArmazenado != null ? new[] { ExameArmazenado } : Array.Empty<Exame>());
        public Task<IEnumerable<Exame>> ListarTodosAsync(ModalidadeExame? modalidade, StatusExame? status) =>
            Task.FromResult<IEnumerable<Exame>>(ExameArmazenado != null ? new[] { ExameArmazenado } : Array.Empty<Exame>());
        public Task<IEnumerable<Exame>> BuscarAsync(string? termoBusca, DateTime? data) =>
            Task.FromResult<IEnumerable<Exame>>(ExameArmazenado != null ? new[] { ExameArmazenado } : Array.Empty<Exame>());
        public Task AdicionarAsync(Exame exame) { ExameArmazenado = exame; return Task.CompletedTask; }
        public Task AtualizarAsync(Exame exame) { ExameArmazenado = exame; return Task.CompletedTask; }
        public Task RemoverAsync(Exame exame) { ExameArmazenado = null; return Task.CompletedTask; }
        public Task<int> SalvarAlteracoesAsync() => Task.FromResult(1);
    }

    private class FakeTriagemRepository : ITriagemRepository
    {
        public FormularioTriagem? FormularioArmazenado { get; set; }

        public Task<FormularioTriagem?> ObterPorIdAsync(int id) => Task.FromResult(FormularioArmazenado);
        public Task<FormularioTriagem?> ObterPorExameIdAsync(int exameId) => Task.FromResult(FormularioArmazenado);
        public Task SalvarAsync(FormularioTriagem formulario) { FormularioArmazenado = formulario; return Task.CompletedTask; }
        public Task AtualizarAsync(FormularioTriagem formulario) { FormularioArmazenado = formulario; return Task.CompletedTask; }
        public Task<int> SalvarAlteracoesAsync() => Task.FromResult(1);
    }

    private class FakePacienteRepository : IPacienteRepository
    {
        public Task<Paciente?> ObterPorIdAsync(int id) => Task.FromResult<Paciente?>(null);
        public Task<Paciente?> ObterPorCpfAsync(string cpf) => Task.FromResult<Paciente?>(null);
        public Task<IEnumerable<Paciente>> BuscarPorNomeAsync(string nome) => Task.FromResult(Enumerable.Empty<Paciente>());
        public Task AdicionarAsync(Paciente paciente) => Task.CompletedTask;
        public Task AtualizarAsync(Paciente paciente) => Task.CompletedTask;
        public Task<int> SalvarAlteracoesAsync() => Task.FromResult(1);
    }

    private class FakeAuditoriaRepository : IAuditoriaRepository
    {
        public List<AuditoriaLog> Logs { get; } = new();
        public Task RegistrarLogAsync(AuditoriaLog log) { Logs.Add(log); return Task.CompletedTask; }
        public Task<IEnumerable<AuditoriaLog>> ListarUltimosLogsAsync(int limite) => Task.FromResult<IEnumerable<AuditoriaLog>>(Logs);
        public Task<IEnumerable<AuditoriaLog>> ListarPorExameAsync(int exameId) => Task.FromResult(Logs.Where(l => l.ExameId == exameId));
        public Task<int> SalvarAlteracoesAsync() => Task.FromResult(1);
    }

    private class FakeUsuarioRepository : IUsuarioRepository
    {
        public Task<Usuario?> ObterPorIdAsync(int id) => Task.FromResult<Usuario?>(new Usuario { Id = id, Nome = "Operador Teste" });
        public Task<Usuario?> ObterPorLoginAsync(string login) => Task.FromResult<Usuario?>(null);
        public Task<IEnumerable<Usuario>> ListarAtivosAsync() => Task.FromResult(Enumerable.Empty<Usuario>());
        public Task<IEnumerable<Usuario>> ListarTodosAsync() => Task.FromResult(Enumerable.Empty<Usuario>());
        public Task AdicionarAsync(Usuario usuario) => Task.CompletedTask;
        public Task AtualizarAsync(Usuario usuario) => Task.CompletedTask;
        public Task<int> SalvarAlteracoesAsync() => Task.FromResult(1);
    }

    private class FakeRelatorioTriagemAppService : TriagemImagem.Application.Interfaces.IRelatorioTriagemAppService
    {
        public Task<string> GerarEArmazenarRelatorioAsync(Exame exame, FormularioTriagem formulario, ResultadoSemaforoClinico avaliacao, string nomeUsuario)
        {
            return Task.FromResult("C:\\Documentos\\Triagens\\Teste.html");
        }
    }

    [Fact]
    public async Task SalvarTriagem_RespostasNormais_DeveLiberarExameParaExecucao()
    {
        // Arrange
        var fakeExameRepo = new FakeExameRepository
        {
            ExameArmazenado = new Exame
            {
                Id = 1,
                Modalidade = ModalidadeExame.TC,
                Status = StatusExame.EM_TRIAGEM,
                Paciente = new Paciente { Id = 10, NomeCompleto = "Paciente Teste", DocumentoCpf = "12345678909", PesoKg = 70 }
            }
        };
        var fakeTriagemRepo = new FakeTriagemRepository();
        var fakePacienteRepo = new FakePacienteRepository();
        var fakeAuditoriaRepo = new FakeAuditoriaRepository();
        var semaforoService = new SemaforoClinicoDomainService();

        var service = new TriagemAppService(
            fakeTriagemRepo, fakeExameRepo, fakePacienteRepo, semaforoService, fakeAuditoriaRepo,
            new FakeUsuarioRepository(), new FakeRelatorioTriagemAppService());

        var request = new SalvarTriagemRequestDto
        {
            ExameId = 1,
            UsuarioResponsavelId = 2,
            PesoKg = 72m,
            VolumeContrasteMl = 108m,
            LoteContraste = "LOT-2026-X",
            Respostas = new List<ItemRespostaDto>
            {
                new() { CodigoPergunta = "TC_ALERGIA_IODO", PerguntaEnunciado = "Alergia iodo?", RespostaValor = "NAO" },
                new() { CodigoPergunta = "TC_JEJUM_4H", PerguntaEnunciado = "Jejum 4h?", RespostaValor = "SIM" }
            }
        };

        // Act
        var resultado = await service.SalvarTriagemAsync(request);

        // Assert
        Assert.NotNull(resultado);
        Assert.Equal(StatusRiscoSemaforo.VERDE, resultado.StatusRisco);
        Assert.False(resultado.BloqueiaExame);
        Assert.Equal(StatusExame.LIBERADO, fakeExameRepo.ExameArmazenado!.Status);
        Assert.Contains(fakeAuditoriaRepo.Logs, l => l.TipoEvento == "TRIAGEM_CONCLUIDA");
    }

    [Fact]
    public async Task SalvarTriagem_ComMarcapassoEmRM_DeveBloquearECancelarExame()
    {
        // Arrange
        var fakeExameRepo = new FakeExameRepository
        {
            ExameArmazenado = new Exame
            {
                Id = 2,
                Modalidade = ModalidadeExame.RM,
                Status = StatusExame.EM_TRIAGEM,
                Paciente = new Paciente { Id = 20, NomeCompleto = "Paciente Cardiopata", DocumentoCpf = "12345678909", PesoKg = 80 }
            }
        };
        var fakeTriagemRepo = new FakeTriagemRepository();
        var fakePacienteRepo = new FakePacienteRepository();
        var fakeAuditoriaRepo = new FakeAuditoriaRepository();
        var semaforoService = new SemaforoClinicoDomainService();

        var service = new TriagemAppService(
            fakeTriagemRepo, fakeExameRepo, fakePacienteRepo, semaforoService, fakeAuditoriaRepo,
            new FakeUsuarioRepository(), new FakeRelatorioTriagemAppService());

        var request = new SalvarTriagemRequestDto
        {
            ExameId = 2,
            UsuarioResponsavelId = 2,
            PesoKg = 80m,
            Respostas = new List<ItemRespostaDto>
            {
                new() { CodigoPergunta = "RM_MARCAPASSO", PerguntaEnunciado = "Possui marca-passo?", RespostaValor = "SIM" }
            }
        };

        // Act
        var resultado = await service.SalvarTriagemAsync(request);

        // Assert
        Assert.NotNull(resultado);
        Assert.Equal(StatusRiscoSemaforo.VERMELHO, resultado.StatusRisco);
        Assert.True(resultado.BloqueiaExame);
        Assert.NotEmpty(resultado.MotivosContraindicacao);
        Assert.Equal(StatusExame.BLOQUEADO_SEGURANCA, fakeExameRepo.ExameArmazenado!.Status);
        Assert.Contains(fakeAuditoriaRepo.Logs, l => l.TipoEvento == "TRIAGEM_CONTRAINDICADA");
    }

    [Fact]
    public async Task SalvarTriagem_ExameInexistente_DeveLancarDomainException()
    {
        // Arrange
        var fakeExameRepo = new FakeExameRepository { ExameArmazenado = null };
        var service = new TriagemAppService(
            new FakeTriagemRepository(), fakeExameRepo, new FakePacienteRepository(), 
            new SemaforoClinicoDomainService(), new FakeAuditoriaRepository(),
            new FakeUsuarioRepository(), new FakeRelatorioTriagemAppService());

        var request = new SalvarTriagemRequestDto
        {
            ExameId = 999,
            UsuarioResponsavelId = 1,
            Respostas = new List<ItemRespostaDto>
            {
                new() { CodigoPergunta = "TC_ALERGIA_IODO", RespostaValor = "NAO" }
            }
        };

        // Act & Assert
        await Assert.ThrowsAsync<DomainException>(() => service.SalvarTriagemAsync(request));
    }
}
