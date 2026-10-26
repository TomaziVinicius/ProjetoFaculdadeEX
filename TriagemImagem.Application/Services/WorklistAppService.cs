using TriagemImagem.Application.DTOs;
using TriagemImagem.Application.Interfaces;
using TriagemImagem.Domain.Entities;
using TriagemImagem.Domain.Enums;
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
        var dataAlvo = filtro.Data ?? DateTime.Today;
        IEnumerable<Exame> exames;

        if (!string.IsNullOrWhiteSpace(filtro.TermoBusca))
        {
            exames = await _exameRepository.BuscarAsync(filtro.TermoBusca, dataAlvo);
            if (filtro.Modalidade.HasValue)
            {
                exames = exames.Where(e => e.Modalidade == filtro.Modalidade.Value);
            }
            if (filtro.Status.HasValue)
            {
                exames = exames.Where(e => e.Status == filtro.Status.Value);
            }
        }
        else
        {
            exames = await _exameRepository.ListarDoDiaAsync(dataAlvo, filtro.Modalidade, filtro.Status);
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
        var exame = await _exameRepository.ObterPorIdAsync(exameId);
        if (exame == null) return null;

        var triagem = await _triagemRepository.ObterPorExameIdAsync(exameId);
        return MapearParaDto(exame, triagem);
    }

    public async Task<bool> ChamarPacienteAsync(int exameId, int usuarioId)
    {
        var exame = await _exameRepository.ObterPorIdAsync(exameId);
        if (exame == null) return false;

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
