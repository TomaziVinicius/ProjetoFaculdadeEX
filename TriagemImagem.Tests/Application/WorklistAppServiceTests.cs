using TriagemImagem.Application.DTOs;
using TriagemImagem.Application.Services;
using TriagemImagem.Domain.Entities;
using TriagemImagem.Domain.Enums;
using TriagemImagem.Domain.Exceptions;
using TriagemImagem.Domain.Repositories;
using Xunit;

namespace TriagemImagem.Tests.Application;

public class WorklistAppServiceTests
{
    private class FakeExameRepository : IExameRepository
    {
        public List<Exame> Exames { get; } = new();

        public Task<Exame?> ObterPorIdAsync(int id) => Task.FromResult(Exames.FirstOrDefault(e => e.Id == id));
        public Task<IEnumerable<Exame>> ListarDoDiaAsync(DateTime data, ModalidadeExame? modalidade, StatusExame? status) =>
            Task.FromResult(Exames.Where(e => (!modalidade.HasValue || e.Modalidade == modalidade) && (!status.HasValue || e.Status == status)).AsEnumerable());
        public Task<IEnumerable<Exame>> ListarTodosAsync(ModalidadeExame? modalidade, StatusExame? status) =>
            Task.FromResult(Exames.Where(e => (!modalidade.HasValue || e.Modalidade == modalidade) && (!status.HasValue || e.Status == status)).AsEnumerable());
        public Task<IEnumerable<Exame>> BuscarAsync(string? termoBusca, DateTime? data) =>
            Task.FromResult(Exames.Where(e => string.IsNullOrWhiteSpace(termoBusca) || e.Paciente.NomeCompleto.Contains(termoBusca, StringComparison.OrdinalIgnoreCase)).AsEnumerable());
        public Task AdicionarAsync(Exame exame) { Exames.Add(exame); return Task.CompletedTask; }
        public Task AtualizarAsync(Exame exame) => Task.CompletedTask;
        public Task RemoverAsync(Exame exame) { Exames.Remove(exame); return Task.CompletedTask; }
        public Task<int> SalvarAlteracoesAsync() => Task.FromResult(1);
    }

    private class FakeTriagemRepository : ITriagemRepository
    {
        public Dictionary<int, FormularioTriagem> Triagens { get; } = new();

        public Task<FormularioTriagem?> ObterPorIdAsync(int id) => Task.FromResult(Triagens.Values.FirstOrDefault(t => t.Id == id));
        public Task<FormularioTriagem?> ObterPorExameIdAsync(int exameId) => Task.FromResult(Triagens.GetValueOrDefault(exameId));
        public Task SalvarAsync(FormularioTriagem formulario) { Triagens[formulario.ExameId] = formulario; return Task.CompletedTask; }
        public Task AtualizarAsync(FormularioTriagem formulario) { Triagens[formulario.ExameId] = formulario; return Task.CompletedTask; }
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

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-100)]
    public async Task ObterExamePorId_ComIdZeroOuNegativo_DeveLancarDomainException(int idInvalido)
    {
        // Arrange
        var service = new WorklistAppService(new FakeExameRepository(), new FakeTriagemRepository(), new FakeAuditoriaRepository());

        // Act & Assert
        var ex = await Assert.ThrowsAsync<DomainException>(() => service.ObterExamePorIdAsync(idInvalido));
        Assert.Contains("maior que zero", ex.Message);
    }

    [Fact]
    public async Task ObterExamePorId_ExameInexistente_DeveRetornarNulo()
    {
        // Arrange
        var service = new WorklistAppService(new FakeExameRepository(), new FakeTriagemRepository(), new FakeAuditoriaRepository());

        // Act
        var resultado = await service.ObterExamePorIdAsync(999999);

        // Assert
        Assert.Null(resultado);
    }

    [Fact]
    public async Task ObterExamePorId_ExameExistente_DeveRetornarDtoMapeado()
    {
        // Arrange
        var exameRepo = new FakeExameRepository();
        var exame = new Exame
        {
            Id = 1,
            Atendimento = 101010,
            Modalidade = ModalidadeExame.RM,
            Status = StatusExame.AGUARDANDO_TRIAGEM,
            Sala = "RM-01",
            DescricaoExame = "Ressonância de Coluna",
            Paciente = new Paciente
            {
                Id = 10,
                NomeCompleto = "Ana Paula Teste",
                DocumentoCpf = "12345678909",
                DataNascimento = new DateTime(1990, 1, 1),
                Sexo = "F",
                PesoKg = 60m
            }
        };
        exameRepo.Exames.Add(exame);
        var service = new WorklistAppService(exameRepo, new FakeTriagemRepository(), new FakeAuditoriaRepository());

        // Act
        var dto = await service.ObterExamePorIdAsync(1);

        // Assert
        Assert.NotNull(dto);
        Assert.Equal(1, dto.Id);
        Assert.Equal(101010, dto.Atendimento);
        Assert.Equal("Ana Paula Teste", dto.PacienteNome);
        Assert.Equal(StatusExame.AGUARDANDO_TRIAGEM, dto.Status);
    }

    [Fact]
    public async Task ListarWorklist_ComModalidadeInvalida_DeveLancarDomainException()
    {
        // Arrange
        var service = new WorklistAppService(new FakeExameRepository(), new FakeTriagemRepository(), new FakeAuditoriaRepository());
        var filtro = new FiltroWorklistDto
        {
            Modalidade = (ModalidadeExame)99
        };

        // Act & Assert
        var ex = await Assert.ThrowsAsync<DomainException>(() => service.ObterWorklistAsync(filtro));
        Assert.Contains("Modalidade de exame inválida", ex.Message);
    }

    [Fact]
    public async Task ListarWorklist_ComStatusInvalido_DeveLancarDomainException()
    {
        // Arrange
        var service = new WorklistAppService(new FakeExameRepository(), new FakeTriagemRepository(), new FakeAuditoriaRepository());
        var filtro = new FiltroWorklistDto
        {
            Status = (StatusExame)99
        };

        // Act & Assert
        var ex = await Assert.ThrowsAsync<DomainException>(() => service.ObterWorklistAsync(filtro));
        Assert.Contains("Status de exame inválido", ex.Message);
    }

    [Fact]
    public async Task ListarWorklist_ComTermoBuscaExcessivo_DeveLancarDomainException()
    {
        // Arrange
        var service = new WorklistAppService(new FakeExameRepository(), new FakeTriagemRepository(), new FakeAuditoriaRepository());
        var filtro = new FiltroWorklistDto
        {
            TermoBusca = new string('A', 101)
        };

        // Act & Assert
        var ex = await Assert.ThrowsAsync<DomainException>(() => service.ObterWorklistAsync(filtro));
        Assert.Contains("não pode exceder 100 caracteres", ex.Message);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task ChamarPaciente_ComExameIdInvalido_DeveLancarDomainException(int exameIdInvalido)
    {
        // Arrange
        var service = new WorklistAppService(new FakeExameRepository(), new FakeTriagemRepository(), new FakeAuditoriaRepository());

        // Act & Assert
        var ex = await Assert.ThrowsAsync<DomainException>(() => service.ChamarPacienteAsync(exameIdInvalido, 1));
        Assert.Contains("maior que zero", ex.Message);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-5)]
    public async Task ChamarPaciente_ComUsuarioIdInvalido_DeveLancarDomainException(int usuarioIdInvalido)
    {
        // Arrange
        var service = new WorklistAppService(new FakeExameRepository(), new FakeTriagemRepository(), new FakeAuditoriaRepository());

        // Act & Assert
        var ex = await Assert.ThrowsAsync<DomainException>(() => service.ChamarPacienteAsync(1, usuarioIdInvalido));
        Assert.Contains("operador responsável", ex.Message);
    }

    [Fact]
    public async Task ChamarPaciente_ExameInexistente_DeveRetornarFalso()
    {
        // Arrange
        var service = new WorklistAppService(new FakeExameRepository(), new FakeTriagemRepository(), new FakeAuditoriaRepository());

        // Act
        var sucesso = await service.ChamarPacienteAsync(999999, 1);

        // Assert
        Assert.False(sucesso);
    }

    [Fact]
    public async Task ChamarPaciente_ExameAguardando_DeveAtualizarStatusEGravarAuditoria()
    {
        // Arrange
        var exameRepo = new FakeExameRepository();
        var auditoriaRepo = new FakeAuditoriaRepository();
        var exame = new Exame
        {
            Id = 1,
            Atendimento = 104520,
            Modalidade = ModalidadeExame.TC,
            Status = StatusExame.AGUARDANDO_TRIAGEM,
            Sala = "TC-01",
            Paciente = new Paciente
            {
                Id = 1,
                NomeCompleto = "Carlos Teste Chamada",
                DocumentoCpf = "12345678909"
            }
        };
        exameRepo.Exames.Add(exame);
        var service = new WorklistAppService(exameRepo, new FakeTriagemRepository(), auditoriaRepo);

        // Act
        var resultado = await service.ChamarPacienteAsync(1, 2);

        // Assert
        Assert.True(resultado);
        Assert.Equal(StatusExame.EM_TRIAGEM, exame.Status);
        Assert.NotNull(exame.HorarioChamada);
        Assert.Contains(auditoriaRepo.Logs, l => l.TipoEvento == "PACIENTE_CHAMADO" && l.UsuarioId == 2);
    }

    [Fact]
    public async Task ChamarPaciente_ExameJaLiberado_DeveLancarDomainException()
    {
        // Arrange
        var exameRepo = new FakeExameRepository();
        var exame = new Exame
        {
            Id = 2,
            Atendimento = 104521,
            Status = StatusExame.LIBERADO,
            Paciente = new Paciente { NomeCompleto = "Paciente Concluido", DocumentoCpf = "12345678909" }
        };
        exameRepo.Exames.Add(exame);
        var service = new WorklistAppService(exameRepo, new FakeTriagemRepository(), new FakeAuditoriaRepository());

        // Act & Assert
        var ex = await Assert.ThrowsAsync<DomainException>(() => service.ChamarPacienteAsync(2, 1));
        Assert.Contains("já se encontra finalizado", ex.Message);
    }

    [Fact]
    public async Task ChamarPaciente_ExameBloqueadoSeguranca_DeveLancarDomainException()
    {
        // Arrange
        var exameRepo = new FakeExameRepository();
        var exame = new Exame
        {
            Id = 3,
            Atendimento = 104522,
            Status = StatusExame.BLOQUEADO_SEGURANCA,
            Paciente = new Paciente { NomeCompleto = "Paciente Bloqueado", DocumentoCpf = "12345678909" }
        };
        exameRepo.Exames.Add(exame);
        var service = new WorklistAppService(exameRepo, new FakeTriagemRepository(), new FakeAuditoriaRepository());

        // Act & Assert
        var ex = await Assert.ThrowsAsync<DomainException>(() => service.ChamarPacienteAsync(3, 1));
        Assert.Contains("bloqueado por contraindicação", ex.Message);
    }
}
