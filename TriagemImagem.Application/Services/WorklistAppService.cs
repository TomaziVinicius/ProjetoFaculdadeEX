using TriagemImagem.Application.DTOs;
using TriagemImagem.Application.Interfaces;
using TriagemImagem.Domain.Entities;
using TriagemImagem.Domain.Enums;
using TriagemImagem.Domain.Exceptions;
using TriagemImagem.Domain.Repositories;

namespace TriagemImagem.Application.Services;

public class WorklistAppService : IWorklistAppService
{
    private readonly IExameRepository _exameRepository;
    private readonly ITriagemRepository _triagemRepository;
    private readonly IAuditoriaRepository _auditoriaRepository;

    public WorklistAppService(
        IExameRepository exameRepository,
        ITriagemRepository triagemRepository,
        IAuditoriaRepository auditoriaRepository)
    {
        _exameRepository = exameRepository;
        _triagemRepository = triagemRepository;
        _auditoriaRepository = auditoriaRepository;
    }

    public async Task<IEnumerable<ExameWorklistDto>> ObterWorklistAsync(FiltroWorklistDto filtro)
    {
        if (filtro.Modalidade.HasValue && !Enum.IsDefined(typeof(ModalidadeExame), filtro.Modalidade.Value))
        {
            throw new DomainException("Modalidade de exame inválida. Utilize 1 para Tomografia Computadorizada (TC) ou 2 para Ressonância Magnética (RM).");
        }

        if (filtro.Status.HasValue && !Enum.IsDefined(typeof(StatusExame), filtro.Status.Value))
        {
            throw new DomainException("Status de exame inválido. Os valores aceitos são de 1 a 5.");
        }

        if (!string.IsNullOrWhiteSpace(filtro.TermoBusca) && filtro.TermoBusca.Length > 100)
        {
            throw new DomainException("O termo de busca não pode exceder 100 caracteres.");
        }

        IEnumerable<Exame> exames;

        if (!string.IsNullOrWhiteSpace(filtro.TermoBusca))
        {
            exames = await _exameRepository.BuscarAsync(filtro.TermoBusca, filtro.Data);
            if (filtro.Modalidade.HasValue)
            {
                exames = exames.Where(e => e.Modalidade == filtro.Modalidade.Value);
            }
            if (filtro.Status.HasValue)
            {
                exames = exames.Where(e => e.Status == filtro.Status.Value);
            }
        }
        else if (filtro.Data.HasValue)
        {
            exames = await _exameRepository.ListarDoDiaAsync(filtro.Data.Value, filtro.Modalidade, filtro.Status);
        }
        else
        {
            // Sem data especificada: primeiro busca os exames de hoje
            exames = await _exameRepository.ListarDoDiaAsync(DateTime.Today, filtro.Modalidade, filtro.Status);
            // Se hoje não houver registros (demonstração, virada de dia), traz todos da fila para não sumir
            if (!exames.Any())
            {
                exames = await _exameRepository.ListarTodosAsync(filtro.Modalidade, filtro.Status);
            }
        }

        var resultado = new List<ExameWorklistDto>();

        foreach (var exame in exames)
        {
            var triagem = await _triagemRepository.ObterPorExameIdAsync(exame.Id);
            resultado.Add(MapearParaDto(exame, triagem));
        }

        return resultado;
    }

    public async Task<ExameWorklistDto?> ObterExamePorIdAsync(int exameId)
    {
        if (exameId <= 0)
        {
            throw new DomainException("O identificador do exame deve ser um número inteiro positivo maior que zero.");
        }

        var exame = await _exameRepository.ObterPorIdAsync(exameId);
        if (exame == null) return null;

        var triagem = await _triagemRepository.ObterPorExameIdAsync(exameId);
        return MapearParaDto(exame, triagem);
    }

    public async Task<bool> ChamarPacienteAsync(int exameId, int usuarioId)
    {
        if (exameId <= 0)
        {
            throw new DomainException("O identificador do exame deve ser um número inteiro positivo maior que zero.");
        }

        if (usuarioId <= 0)
        {
            throw new DomainException("É obrigatório informar o identificador do operador responsável pela chamada.");
        }

        var exame = await _exameRepository.ObterPorIdAsync(exameId);
        if (exame == null) return false;

        if (exame.Status == StatusExame.LIBERADO || exame.Status == StatusExame.CONCLUIDO)
        {
            throw new DomainException($"Não é permitido chamar o paciente pois o exame #{exame.Atendimento} já se encontra finalizado ({exame.Status}).");
        }

        if (exame.Status == StatusExame.BLOQUEADO_SEGURANCA)
        {
            throw new DomainException($"Não é permitido chamar o paciente pois o exame #{exame.Atendimento} foi bloqueado por contraindicação clínica de segurança.");
        }

        exame.HorarioChamada = DateTime.UtcNow;
        if (exame.Status == StatusExame.AGUARDANDO_TRIAGEM)
        {
            exame.Status = StatusExame.EM_TRIAGEM;
        }

        await _exameRepository.AtualizarAsync(exame);
        await _exameRepository.SalvarAlteracoesAsync();

        await _auditoriaRepository.RegistrarLogAsync(new AuditoriaLog
        {
            UsuarioId = usuarioId,
            ExameId = exameId,
            TipoEvento = "PACIENTE_CHAMADO",
            Detalhes = $"Paciente '{exame.Paciente.NomeCompleto}' chamado para a sala '{exame.Sala}'."
        });

        return true;
    }

    private static ExameWorklistDto MapearParaDto(Exame exame, FormularioTriagem? triagem)
    {
        return new ExameWorklistDto
        {
            Id = exame.Id,
            Atendimento = exame.Atendimento,
            PacienteId = exame.PacienteId,
            PacienteNome = exame.Paciente.NomeCompleto,
            PacienteCpf = exame.Paciente.DocumentoCpf,
            Idade = exame.Paciente.ObterIdade(),
            Sexo = exame.Paciente.Sexo,
            PesoKg = exame.Paciente.PesoKg,
            AlturaCm = exame.Paciente.AlturaCm,
            Modalidade = exame.Modalidade,
            CodigoProcedimento = exame.CodigoProcedimento,
            DescricaoExame = exame.DescricaoExame,
            TipoFormulario = exame.TipoFormulario ?? (exame.Modalidade == ModalidadeExame.RM ? "RM_COLUNA" : "TC_TORAX"),
            SegmentoLado = exame.SegmentoLado,
            MotivoExame = exame.MotivoExame,
            MedicoSolicitante = exame.MedicoSolicitante,
            Convenio = exame.Convenio,
            Sala = exame.Sala,
            HorarioAgendado = exame.HorarioAgendado,
            HorarioChamada = exame.HorarioChamada,
            Status = exame.Status,
            PossuiTriagem = triagem != null,
            StatusRiscoSemaforo = triagem?.StatusRisco,
            VolumeContrasteMl = triagem?.VolumeContrasteMl
        };
    }
}
