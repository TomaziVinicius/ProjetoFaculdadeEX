using TriagemImagem.Application.DTOs;
using TriagemImagem.Application.Interfaces;
using TriagemImagem.Application.Validators;
using TriagemImagem.Domain.Entities;
using TriagemImagem.Domain.Enums;
using TriagemImagem.Domain.Exceptions;
using TriagemImagem.Domain.Repositories;
using TriagemImagem.Domain.Services;

namespace TriagemImagem.Application.Services;

public class TriagemAppService : ITriagemAppService
{
    private readonly ITriagemRepository _triagemRepository;
    private readonly IExameRepository _exameRepository;
    private readonly IPacienteRepository _pacienteRepository;
    private readonly ISemaforoClinicoDomainService _semaforoDomainService;
    private readonly IAuditoriaRepository _auditoriaRepository;
    private readonly IUsuarioRepository _usuarioRepository;
    private readonly IRelatorioTriagemAppService _relatorioService;

    public TriagemAppService(
        ITriagemRepository triagemRepository,
        IExameRepository exameRepository,
        IPacienteRepository pacienteRepository,
        ISemaforoClinicoDomainService semaforoDomainService,
        IAuditoriaRepository auditoriaRepository,
        IUsuarioRepository usuarioRepository,
        IRelatorioTriagemAppService relatorioService)
    {
        _triagemRepository = triagemRepository;
        _exameRepository = exameRepository;
        _pacienteRepository = pacienteRepository;
        _semaforoDomainService = semaforoDomainService;
        _auditoriaRepository = auditoriaRepository;
        _usuarioRepository = usuarioRepository;
        _relatorioService = relatorioService;
    }

    public async Task<DetalhesTriagemDto> SalvarTriagemAsync(SalvarTriagemRequestDto request)
    {
        // 1. Validação de formato e preenchimento
        TriagemValidator.Validar(request);

        // 2. Busca do exame associado
        var exame = await _exameRepository.ObterPorIdAsync(request.ExameId);
        if (exame == null)
            throw new DomainException($"Exame com ID {request.ExameId} não foi encontrado.");

        // Atualiza peso do paciente se informado no formulário
        if (request.PesoKg.HasValue && request.PesoKg.Value > 0)
        {
            exame.Paciente.PesoKg = request.PesoKg.Value;
            await _pacienteRepository.AtualizarAsync(exame.Paciente);
        }

        // 3. Converte itens de resposta para entidades de domínio
        var itensEntidade = request.Respostas.Select(r => new ItemRespostaTriagem
        {
            CodigoPergunta = r.CodigoPergunta,
            PerguntaEnunciado = r.PerguntaEnunciado,
            RespostaValor = r.RespostaValor.Trim().ToUpperInvariant(),
            Impeditivo = r.Impeditivo,
            Justificativa = r.Justificativa
        }).ToList();

        // 4. Executa a avaliação de segurança clínica (Semáforo de Risco)
        var avaliacao = _semaforoDomainService.Avaliar(exame.Modalidade, itensEntidade, request.PesoKg);

        // 5. Criação ou atualização do formulário de triagem
        var formulario = await _triagemRepository.ObterPorExameIdAsync(exame.Id) 
            ?? new FormularioTriagem { ExameId = exame.Id };

        formulario.UsuarioResponsavelId = request.UsuarioResponsavelId;
        formulario.StatusRisco = avaliacao.StatusRisco;
        formulario.VolumeContrasteMl = request.VolumeContrasteMl;
        formulario.LoteContraste = request.LoteContraste;
        formulario.ObservacoesClinicas = request.ObservacoesClinicas;
        formulario.FinalizadoEm = DateTime.UtcNow;

        formulario.Respostas.Clear();
        foreach (var item in itensEntidade)
        {
            formulario.Respostas.Add(item);
        }

        // 6. Atualização do status do Exame conforme Semáforo Clínico
        if (avaliacao.BloqueiaExame)
        {
            exame.Status = StatusExame.BLOQUEADO_SEGURANCA;
        }
        else
        {
            exame.Status = StatusExame.LIBERADO;
        }

        await _triagemRepository.SalvarAsync(formulario);
        await _triagemRepository.SalvarAlteracoesAsync();

        await _exameRepository.AtualizarAsync(exame);
        await _exameRepository.SalvarAlteracoesAsync();

        // 7. Geração e Arquivamento da Ficha Clínica na pasta Documentos
        try
        {
            var usuario = await _usuarioRepository.ObterPorIdAsync(request.UsuarioResponsavelId);
            var nomeOperador = usuario?.Nome ?? "Equipe de Biomedicina";
            var caminhoArquivo = await _relatorioService.GerarEArmazenarRelatorioAsync(exame, formulario, avaliacao, nomeOperador);
            formulario.CaminhoArquivoArmazenado = caminhoArquivo;
            await _triagemRepository.SalvarAsync(formulario);
            await _triagemRepository.SalvarAlteracoesAsync();
        }
        catch
        {
            // Fallback de permissão de pasta sem interromper a transação da triagem
        }

        // 8. Registro de Trilha de Auditoria
        var eventoTipo = avaliacao.BloqueiaExame ? "TRIAGEM_CONTRAINDICADA" : "TRIAGEM_CONCLUIDA";
        var resumoAlertas = avaliacao.BloqueiaExame 
            ? string.Join(" | ", avaliacao.MotivosContraindicacao)
            : (avaliacao.AlertasAtencao.Any() ? string.Join(" | ", avaliacao.AlertasAtencao) : "Sem contraindicações clínicas.");

        await _auditoriaRepository.RegistrarLogAsync(new AuditoriaLog
        {
            UsuarioId = request.UsuarioResponsavelId,
            ExameId = exame.Id,
            TipoEvento = eventoTipo,
            Detalhes = $"Triagem finalizada com status '{avaliacao.StatusRisco}'. Arquivo salvo: '{formulario.CaminhoArquivoArmazenado}'. {resumoAlertas}"
        });

        return new DetalhesTriagemDto
        {
            Id = formulario.Id,
            ExameId = exame.Id,
            UsuarioResponsavelId = request.UsuarioResponsavelId,
            UsuarioResponsavelNome = formulario.UsuarioResponsavel?.Nome ?? string.Empty,
            StatusRisco = avaliacao.StatusRisco,
            VolumeContrasteMl = formulario.VolumeContrasteMl,
            LoteContraste = formulario.LoteContraste,
            ObservacoesClinicas = formulario.ObservacoesClinicas,
            IniciadoEm = formulario.IniciadoEm,
            FinalizadoEm = formulario.FinalizadoEm,
            CaminhoArquivoArmazenado = formulario.CaminhoArquivoArmazenado,
            MotivosContraindicacao = avaliacao.MotivosContraindicacao.ToList(),
            AlertasAtencao = avaliacao.AlertasAtencao.ToList(),
            Respostas = request.Respostas
        };
    }

    public async Task<DetalhesTriagemDto?> ObterPorExameIdAsync(int exameId)
    {
        var formulario = await _triagemRepository.ObterPorExameIdAsync(exameId);
        if (formulario == null) return null;

        var avaliacao = _semaforoDomainService.Avaliar(
            formulario.Exame.Modalidade, 
            formulario.Respostas, 
            formulario.Exame.Paciente.PesoKg);

        return new DetalhesTriagemDto
        {
            Id = formulario.Id,
            ExameId = formulario.ExameId,
            UsuarioResponsavelId = formulario.UsuarioResponsavelId,
            UsuarioResponsavelNome = formulario.UsuarioResponsavel?.Nome ?? string.Empty,
            StatusRisco = formulario.StatusRisco,
            VolumeContrasteMl = formulario.VolumeContrasteMl,
            LoteContraste = formulario.LoteContraste,
            ObservacoesClinicas = formulario.ObservacoesClinicas,
            IniciadoEm = formulario.IniciadoEm,
            FinalizadoEm = formulario.FinalizadoEm,
            CaminhoArquivoArmazenado = formulario.CaminhoArquivoArmazenado,
            MotivosContraindicacao = avaliacao.MotivosContraindicacao.ToList(),
            AlertasAtencao = avaliacao.AlertasAtencao.ToList(),
            Respostas = formulario.Respostas.Select(r => new ItemRespostaDto
            {
                CodigoPergunta = r.CodigoPergunta,
                PerguntaEnunciado = r.PerguntaEnunciado,
                RespostaValor = r.RespostaValor,
                Impeditivo = r.Impeditivo,
                Justificativa = r.Justificativa
            }).ToList()
        };
    }

    public async Task<bool> LiberarExameAsync(int exameId, int usuarioId, string? observacao = null)
    {
        var exame = await _exameRepository.ObterPorIdAsync(exameId);
        if (exame == null) return false;

        var triagem = await _triagemRepository.ObterPorExameIdAsync(exameId);
        if (triagem?.StatusRisco == StatusRiscoSemaforo.VERMELHO)
            throw new DomainException("Não é permitido liberar para execução um exame com Semáforo Vermelho (Contraindicação clínica impeditiva).");

        exame.Status = StatusExame.LIBERADO;
        await _exameRepository.AtualizarAsync(exame);
        await _exameRepository.SalvarAlteracoesAsync();

        await _auditoriaRepository.RegistrarLogAsync(new AuditoriaLog
        {
            UsuarioId = usuarioId,
            ExameId = exameId,
            TipoEvento = "EXAME_LIBERADO",
            Detalhes = $"Exame liberado para execução pelo operador. {observacao}".Trim()
        });

        return true;
    }

    public async Task<bool> CancelarExameAsync(int exameId, int usuarioId, string motivo)
    {
        var exame = await _exameRepository.ObterPorIdAsync(exameId);
        if (exame == null) return false;

        exame.Status = StatusExame.BLOQUEADO_SEGURANCA;
        await _exameRepository.AtualizarAsync(exame);
        await _exameRepository.SalvarAlteracoesAsync();

        await _auditoriaRepository.RegistrarLogAsync(new AuditoriaLog
        {
            UsuarioId = usuarioId,
            ExameId = exameId,
            TipoEvento = "EXAME_BLOQUEADO_SEGURANCA",
            Detalhes = $"Exame BLOQUEADO_SEGURANCA pelo operador. Motivo: {motivo}"
        });

        return true;
    }
}
