namespace TriagemImagem.Domain.Services;

public sealed record TasyStatusDto(
    bool Conectado,
    long LatenciaMs,
    string VersaoOracle,
    string Servidor,
    string Servico,
    string Usuario,
    int TotalProcedimentos,
    string? MensagemErro
);

public sealed record ProcedimentoTasyDto(
    long Sequencia,
    string Descricao,
    long? CodigoProcedimento,
    string ModalidadeEstimada
);

public sealed record TasyQueryResultDto(
    bool Sucesso,
    long TempoExecucaoMs,
    int TotalLinhas,
    IReadOnlyList<string> Colunas,
    IReadOnlyList<Dictionary<string, object?>> Linhas,
    string? MensagemErro
);

public sealed record ExameTasyDiaDto(
    long Atendimento,
    string Prontuario,
    string PacienteNome,
    DateTime? DataNascimento,
    int? Idade,
    string? Sexo,
    string? Cpf,
    string? Telefone,
    DateTime? DataEntrada,
    long? Prescricao,
    string? Convenio,
    string? MedicoSolicitante,
    long? CodigoProcedimento,
    string ProcedimentoNome,
    long? SequenciaProcedimentoInterno,
    long? SetorAtendimento,
    string Modalidade,
    string? StatusExecucaoCodigo,
    string StatusExecucaoDescricao
)
{
    public long? NrSeqProcInterno => SequenciaProcedimentoInterno;
}

public interface ITasyService
{
    Task<TasyStatusDto> TestarConexaoAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ProcedimentoTasyDto>> ConsultarProcedimentosAsync(
        string? busca = null,
        long? codigo = null,
        int limite = 50,
        int offset = 0,
        CancellationToken cancellationToken = default);

    Task<int> ObterTotalProcedimentosAsync(string? busca = null, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ExameTasyDiaDto>> ConsultarExamesDiaAsync(
        DateTime? data = null,
        string? busca = null,
        int limite = 50,
        int offset = 0,
        CancellationToken cancellationToken = default);

    Task<TasyQueryResultDto> ExecutarSelectCustomizadoAsync(
        string sqlQuery,
        int limiteMaximo = 100,
        CancellationToken cancellationToken = default);
}
