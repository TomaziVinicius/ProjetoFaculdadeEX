using System.Data;
using System.Diagnostics;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Oracle.ManagedDataAccess.Client;
using TriagemImagem.Domain.Configurations;
using TriagemImagem.Domain.Services;

namespace TriagemImagem.Infrastructure.Services;

public sealed class TasyService : ITasyService
{
    private readonly TasySettings _settings;
    private readonly ILogger<TasyService> _logger;

    private static readonly Regex DisallowedSqlPattern = new(
        @"\b(INSERT|UPDATE|DELETE|DROP|ALTER|TRUNCATE|CREATE|GRANT|REVOKE|MERGE|EXEC|EXECUTE)\b",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    public TasyService(IOptions<TasySettings> options, ILogger<TasyService> logger)
    {
        _settings = options.Value;
        _logger = logger;
    }

    public async Task<TasyStatusDto> TestarConexaoAsync(CancellationToken cancellationToken = default)
    {
        var sw = Stopwatch.StartNew();
        var versaoOracle = "Oracle Database";
        var totalProcedimentos = 0;

        try
        {
            await using var conn = new OracleConnection(_settings.BuildConnectionString());
            await conn.OpenAsync(cancellationToken);

            // 1. Versão do Banco
            try
            {
                await using var cmdVer = conn.CreateCommand();
                cmdVer.CommandText = "SELECT banner FROM v$version WHERE ROWNUM <= 1";
                var banner = await cmdVer.ExecuteScalarAsync(cancellationToken);
                if (banner != null && banner != DBNull.Value)
                {
                    versaoOracle = banner.ToString() ?? versaoOracle;
                }
            }
            catch
            {
                await using var cmdDual = conn.CreateCommand();
                cmdDual.CommandText = "SELECT 1 FROM DUAL";
                await cmdDual.ExecuteScalarAsync(cancellationToken);
                versaoOracle = $"Oracle Server {conn.ServerVersion}";
            }

            // 2. Contagem da view de procedimentos
            try
            {
                await using var cmdCount = conn.CreateCommand();
                cmdCount.CommandText = $"SELECT COUNT(*) FROM {_settings.ProcedimentosView}";
                var countRes = await cmdCount.ExecuteScalarAsync(cancellationToken);
                if (countRes != null && countRes != DBNull.Value)
                {
                    totalProcedimentos = Convert.ToInt32(countRes);
                }
            }
            catch (Exception exCount)
            {
                _logger.LogWarning(exCount, "Aviso ao contar procedimentos em {View}", _settings.ProcedimentosView);
            }

            sw.Stop();
            _logger.LogInformation("Conexão Tasy Oracle validada com sucesso em {Ms}ms. Procedimentos: {Count}",
                sw.ElapsedMilliseconds, totalProcedimentos);

            return new TasyStatusDto(
                Conectado: true,
                LatenciaMs: sw.ElapsedMilliseconds,
                VersaoOracle: versaoOracle,
                Servidor: $"{_settings.Host}:{_settings.Port}",
                Servico: _settings.ServiceName,
                Usuario: _settings.User,
                TotalProcedimentos: totalProcedimentos,
                MensagemErro: null
            );
        }
        catch (Exception ex)
        {
            sw.Stop();
            _logger.LogError(ex, "Falha ao conectar no banco Oracle Tasy");

            var msg = ex.Message;
            if (msg.Contains("ORA-01017"))
            {
                msg = "Usuário ou senha inválidos no Oracle Tasy (ORA-01017).";
            }
            else if (msg.Contains("ORA-12170") || msg.Contains("ORA-12541") || msg.Contains("timed out"))
            {
                msg = $"Tempo limite esgotado ao contatar o servidor Oracle em {_settings.Host}:{_settings.Port}. Verifique se a rede/firewall permite a porta.";
            }
            else if (msg.Contains("ORA-12514"))
            {
                msg = $"Serviço '{_settings.ServiceName}' não encontrado no Listener Oracle em {_settings.Host}:{_settings.Port}.";
            }

            return new TasyStatusDto(
                Conectado: false,
                LatenciaMs: sw.ElapsedMilliseconds,
                VersaoOracle: "Indisponível",
                Servidor: $"{_settings.Host}:{_settings.Port}",
                Servico: _settings.ServiceName,
                Usuario: _settings.User,
                TotalProcedimentos: 0,
                MensagemErro: msg
            );
        }
    }

    public async Task<IReadOnlyList<ProcedimentoTasyDto>> ConsultarProcedimentosAsync(
        string? busca = null,
        long? codigo = null,
        int limite = 50,
        int offset = 0,
        CancellationToken cancellationToken = default)
    {
        var lista = new List<ProcedimentoTasyDto>();
        limite = Math.Clamp(limite, 1, 500);
        offset = Math.Max(0, offset);

        await using var conn = new OracleConnection(_settings.BuildConnectionString());
        await conn.OpenAsync(cancellationToken);

        await using var cmd = conn.CreateCommand();
        cmd.BindByName = true;

        var whereClauses = new List<string>();

        if (codigo.HasValue)
        {
            whereClauses.Add("(CD_PROCEDIMENTO = :p_cod OR NR_SEQUENCIA = :p_cod)");
            cmd.Parameters.Add(new OracleParameter("p_cod", OracleDbType.Int64, codigo.Value, ParameterDirection.Input));
        }

        if (!string.IsNullOrWhiteSpace(busca))
        {
            whereClauses.Add("UPPER(DS_PROC_EXAME) LIKE :p_busca");
            cmd.Parameters.Add(new OracleParameter("p_busca", OracleDbType.Varchar2, $"%{busca.Trim().ToUpperInvariant()}%", ParameterDirection.Input));
        }

        var whereSql = whereClauses.Count > 0 ? $"WHERE {string.Join(" AND ", whereClauses)}" : string.Empty;

        cmd.CommandText = $@"
            SELECT NR_SEQUENCIA, DS_PROC_EXAME, CD_PROCEDIMENTO
            FROM {_settings.ProcedimentosView}
            {whereSql}
            ORDER BY DS_PROC_EXAME ASC
            OFFSET :p_offset ROWS FETCH NEXT :p_limite ROWS ONLY";

        cmd.Parameters.Add(new OracleParameter("p_offset", OracleDbType.Int32, offset, ParameterDirection.Input));
        cmd.Parameters.Add(new OracleParameter("p_limite", OracleDbType.Int32, limite, ParameterDirection.Input));

        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            var seq = reader.IsDBNull(0) ? 0L : Convert.ToInt64(reader.GetValue(0));
            var desc = reader.IsDBNull(1) ? string.Empty : reader.GetString(1);
            long? cod = reader.IsDBNull(2) ? null : Convert.ToInt64(reader.GetValue(2));

            lista.Add(new ProcedimentoTasyDto(
                Sequencia: seq,
                Descricao: desc,
                CodigoProcedimento: cod,
                ModalidadeEstimada: EstimarModalidade(desc)
            ));
        }

        return lista;
    }

    public async Task<int> ObterTotalProcedimentosAsync(string? busca = null, CancellationToken cancellationToken = default)
    {
        await using var conn = new OracleConnection(_settings.BuildConnectionString());
        await conn.OpenAsync(cancellationToken);

        await using var cmd = conn.CreateCommand();

        var whereSql = string.Empty;
        if (!string.IsNullOrWhiteSpace(busca))
        {
            whereSql = "WHERE UPPER(DS_PROC_EXAME) LIKE :p_busca";
            cmd.Parameters.Add(new OracleParameter("p_busca", OracleDbType.Varchar2, $"%{busca.Trim().ToUpperInvariant()}%", ParameterDirection.Input));
        }

        cmd.CommandText = $"SELECT COUNT(*) FROM {_settings.ProcedimentosView} {whereSql}";
        var res = await cmd.ExecuteScalarAsync(cancellationToken);
        return res != null && res != DBNull.Value ? Convert.ToInt32(res) : 0;
    }

    public async Task<IReadOnlyList<ExameTasyDiaDto>> ConsultarExamesDiaAsync(
        DateTime? data = null,
        string? busca = null,
        int limite = 50,
        int offset = 0,
        CancellationToken cancellationToken = default)
    {
        limite = Math.Clamp(limite, 1, 200);
        offset = Math.Max(0, offset);

        await using var conn = new OracleConnection(_settings.BuildConnectionString());
        await conn.OpenAsync(cancellationToken);

        await using var cmd = conn.CreateCommand();
        cmd.BindByName = true;

        var conditions = new List<string>();

        // 1. Filtro por data de atendimento/entrada
        if (data.HasValue)
        {
            var dtInicio = data.Value.Date;
            var dtFim = dtInicio.AddDays(1);
            conditions.Add("DT_ENTRADA >= :p_dt_inicio AND DT_ENTRADA < :p_dt_fim");
            cmd.Parameters.Add(new OracleParameter("p_dt_inicio", OracleDbType.Date, dtInicio, ParameterDirection.Input));
            cmd.Parameters.Add(new OracleParameter("p_dt_fim", OracleDbType.Date, dtFim, ParameterDirection.Input));
        }

        // 2. Filtro textual opcional (Nome do paciente, CPF, Atendimento ou Prontuário)
        if (!string.IsNullOrWhiteSpace(busca))
        {
            var termo = busca.Trim();
            var apenasDigitos = new string(termo.Where(char.IsDigit).ToArray());

            if (apenasDigitos.Length >= 4 && long.TryParse(apenasDigitos, out var numBusca))
            {
                conditions.Add("(NR_ATENDIMENTO = :p_num OR NR_CPF LIKE :p_cpf_like OR CD_PESSOA_FISICA = :p_pront OR UPPER(NM_PESSOA_FISICA) LIKE :p_busca_nome)");
                cmd.Parameters.Add(new OracleParameter("p_num", OracleDbType.Int64, numBusca, ParameterDirection.Input));
                cmd.Parameters.Add(new OracleParameter("p_cpf_like", OracleDbType.Varchar2, $"%{apenasDigitos}%", ParameterDirection.Input));
                cmd.Parameters.Add(new OracleParameter("p_pront", OracleDbType.Varchar2, termo, ParameterDirection.Input));
                cmd.Parameters.Add(new OracleParameter("p_busca_nome", OracleDbType.Varchar2, $"%{termo.ToUpperInvariant()}%", ParameterDirection.Input));
            }
            else
            {
                conditions.Add("(UPPER(NM_PESSOA_FISICA) LIKE :p_busca_nome OR NR_CPF LIKE :p_cpf_like OR CD_PESSOA_FISICA = :p_pront)");
                cmd.Parameters.Add(new OracleParameter("p_busca_nome", OracleDbType.Varchar2, $"%{termo.ToUpperInvariant()}%", ParameterDirection.Input));
                cmd.Parameters.Add(new OracleParameter("p_cpf_like", OracleDbType.Varchar2, $"%{termo}%", ParameterDirection.Input));
                cmd.Parameters.Add(new OracleParameter("p_pront", OracleDbType.Varchar2, termo, ParameterDirection.Input));
            }
        }

        var whereClause = conditions.Count > 0 ? "WHERE " + string.Join(" AND ", conditions) : string.Empty;

        var sql = $@"
            SELECT 
                NR_ATENDIMENTO,
                CD_PESSOA_FISICA,
                NM_PESSOA_FISICA,
                DT_NASCIMENTO,
                IE_SEXO,
                NR_CPF,
                NR_TELEFONE_CELULAR,
                DT_ENTRADA,
                NR_PRESCRICAO,
                DS_CONVENIO,
                DS_MEDICO_SOLICITANTE,
                CD_PROCEDIMENTO,
                DS_PROCEDIMENTO,
                NR_SEQ_PROC_INTERNO,
                CD_SETOR_ATENDIMENTO,
                IE_STATUS_EXECUCAO
            FROM {_settings.ExamesDiaView}
            {whereClause}
            ORDER BY DT_ENTRADA DESC
            OFFSET :p_offset ROWS FETCH NEXT :p_limite ROWS ONLY";

        cmd.Parameters.Add(new OracleParameter("p_offset", OracleDbType.Int32, offset, ParameterDirection.Input));
        cmd.Parameters.Add(new OracleParameter("p_limite", OracleDbType.Int32, limite, ParameterDirection.Input));
        cmd.CommandText = sql;
        cmd.CommandTimeout = 15;

        var lista = new List<ExameTasyDiaDto>();
        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);

        while (await reader.ReadAsync(cancellationToken))
        {
            var atendimento = reader.GetInt64(reader.GetOrdinal("NR_ATENDIMENTO"));
            var prontuario = reader.IsDBNull(reader.GetOrdinal("CD_PESSOA_FISICA")) ? string.Empty : reader.GetString(reader.GetOrdinal("CD_PESSOA_FISICA"));
            var pacienteNome = reader.IsDBNull(reader.GetOrdinal("NM_PESSOA_FISICA")) ? "Paciente Não Identificado" : reader.GetString(reader.GetOrdinal("NM_PESSOA_FISICA")).Trim();
            
            DateTime? dtNasc = reader.IsDBNull(reader.GetOrdinal("DT_NASCIMENTO")) ? null : reader.GetDateTime(reader.GetOrdinal("DT_NASCIMENTO"));
            int? idade = null;
            if (dtNasc.HasValue)
            {
                var hoje = DateTime.Today;
                idade = hoje.Year - dtNasc.Value.Year;
                if (dtNasc.Value.Date > hoje.AddYears(-idade.Value)) idade--;
            }

            var sexo = reader.IsDBNull(reader.GetOrdinal("IE_SEXO")) ? null : reader.GetString(reader.GetOrdinal("IE_SEXO")).Trim().ToUpperInvariant();
            var cpf = reader.IsDBNull(reader.GetOrdinal("NR_CPF")) ? null : reader.GetString(reader.GetOrdinal("NR_CPF")).Trim();
            var telefone = reader.IsDBNull(reader.GetOrdinal("NR_TELEFONE_CELULAR")) ? null : reader.GetString(reader.GetOrdinal("NR_TELEFONE_CELULAR")).Trim();
            var dtEntrada = reader.IsDBNull(reader.GetOrdinal("DT_ENTRADA")) ? null : (DateTime?)reader.GetDateTime(reader.GetOrdinal("DT_ENTRADA"));
            var prescricao = reader.IsDBNull(reader.GetOrdinal("NR_PRESCRICAO")) ? null : (long?)reader.GetInt64(reader.GetOrdinal("NR_PRESCRICAO"));
            
            var convenio = reader.IsDBNull(reader.GetOrdinal("DS_CONVENIO")) ? null : reader.GetString(reader.GetOrdinal("DS_CONVENIO")).Trim();
            var medico = reader.IsDBNull(reader.GetOrdinal("DS_MEDICO_SOLICITANTE")) ? null : reader.GetString(reader.GetOrdinal("DS_MEDICO_SOLICITANTE")).Trim();
            var codProc = reader.IsDBNull(reader.GetOrdinal("CD_PROCEDIMENTO")) ? null : (long?)reader.GetInt64(reader.GetOrdinal("CD_PROCEDIMENTO"));
            var procNome = reader.IsDBNull(reader.GetOrdinal("DS_PROCEDIMENTO")) ? $"Procedimento Tasy ({codProc})" : reader.GetString(reader.GetOrdinal("DS_PROCEDIMENTO")).Trim();
            var seqInterno = reader.IsDBNull(reader.GetOrdinal("NR_SEQ_PROC_INTERNO")) ? null : (long?)reader.GetInt64(reader.GetOrdinal("NR_SEQ_PROC_INTERNO"));
            var setor = reader.IsDBNull(reader.GetOrdinal("CD_SETOR_ATENDIMENTO")) ? null : (long?)reader.GetInt64(reader.GetOrdinal("CD_SETOR_ATENDIMENTO"));
            
            var statusCod = reader.IsDBNull(reader.GetOrdinal("IE_STATUS_EXECUCAO")) ? null : reader.GetString(reader.GetOrdinal("IE_STATUS_EXECUCAO")).Trim();
            var statusDesc = statusCod switch
            {
                "10" => "Prescrito",
                "11" => "Chegando ao Setor / Em Exame",
                "20" => "Executado",
                _ => string.IsNullOrWhiteSpace(statusCod) ? "Aguardando" : $"Status {statusCod}"
            };

            // Setor 17 no Tasy representa Ressonância (RM)
            var modalidade = setor == 17 ? "RM" : (procNome.StartsWith("TC", StringComparison.OrdinalIgnoreCase) ? "TC" : "RM");

            lista.Add(new ExameTasyDiaDto(
                atendimento,
                prontuario,
                pacienteNome,
                dtNasc,
                idade,
                sexo,
                cpf,
                telefone,
                dtEntrada,
                prescricao,
                convenio,
                medico,
                codProc,
                procNome,
                seqInterno,
                setor,
                modalidade,
                statusCod,
                statusDesc
            ));
        }

        return lista;
    }

    public async Task<TasyQueryResultDto> ExecutarSelectCustomizadoAsync(
        string sqlQuery,
        int limiteMaximo = 100,
        CancellationToken cancellationToken = default)
    {
        var sw = Stopwatch.StartNew();
        limiteMaximo = Math.Clamp(limiteMaximo, 1, 200);

        var cleanedQuery = sqlQuery.Trim().TrimEnd(';', ' ');

        if (string.IsNullOrWhiteSpace(cleanedQuery))
        {
            return new TasyQueryResultDto(false, 0, 0, Array.Empty<string>(), Array.Empty<Dictionary<string, object?>>(), "A instrução SQL não pode ser vazia.");
        }

        // Validação estrita de SELECT
        if (!cleanedQuery.StartsWith("SELECT", StringComparison.OrdinalIgnoreCase) &&
            !cleanedQuery.StartsWith("WITH", StringComparison.OrdinalIgnoreCase))
        {
            return new TasyQueryResultDto(false, 0, 0, Array.Empty<string>(), Array.Empty<Dictionary<string, object?>>(),
                "Apenas instruções de consulta (SELECT) são permitidas no console administrativo.");
        }

        if (DisallowedSqlPattern.IsMatch(cleanedQuery))
        {
            return new TasyQueryResultDto(false, 0, 0, Array.Empty<string>(), Array.Empty<Dictionary<string, object?>>(),
                "A instrução contém palavras-chave não permitidas para operações de consulta segura.");
        }

        try
        {
            await using var conn = new OracleConnection(_settings.BuildConnectionString());
            await conn.OpenAsync(cancellationToken);

            // Envolve em subquery com ROWNUM para proteger consumo de memória
            var safeSql = $"SELECT * FROM ({cleanedQuery}) WHERE ROWNUM <= {limiteMaximo}";

            await using var cmd = conn.CreateCommand();
            cmd.CommandText = safeSql;
            cmd.CommandTimeout = 15;

            await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);

            var colunas = new List<string>();
            for (var i = 0; i < reader.FieldCount; i++)
            {
                colunas.Add(reader.GetName(i));
            }

            var linhas = new List<Dictionary<string, object?>>();
            while (await reader.ReadAsync(cancellationToken))
            {
                var linha = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);
                for (var i = 0; i < reader.FieldCount; i++)
                {
                    var colName = reader.GetName(i);
                    if (reader.IsDBNull(i))
                    {
                        linha[colName] = null;
                    }
                    else
                    {
                        var val = reader.GetValue(i);
                        linha[colName] = val switch
                        {
                            DateTime dt => dt.ToString("yyyy-MM-dd HH:mm:ss"),
                            byte[] bytes => $"[Binary {bytes.Length} bytes]",
                            _ => val.ToString()
                        };
                    }
                }
                linhas.Add(linha);
            }

            sw.Stop();
            return new TasyQueryResultDto(
                Sucesso: true,
                TempoExecucaoMs: sw.ElapsedMilliseconds,
                TotalLinhas: linhas.Count,
                Colunas: colunas,
                Linhas: linhas,
                MensagemErro: null
            );
        }
        catch (Exception ex)
        {
            sw.Stop();
            _logger.LogError(ex, "Erro ao executar consulta SQL customizada no Tasy");
            return new TasyQueryResultDto(
                Sucesso: false,
                TempoExecucaoMs: sw.ElapsedMilliseconds,
                TotalLinhas: 0,
                Colunas: Array.Empty<string>(),
                Linhas: Array.Empty<Dictionary<string, object?>>(),
                MensagemErro: ex.Message
            );
        }
    }

    private static string EstimarModalidade(string descricao)
    {
        if (string.IsNullOrWhiteSpace(descricao)) return "OUTROS";
        var upper = descricao.ToUpperInvariant();

        if (upper.StartsWith("RM ") || upper.StartsWith("MR ") || upper.Contains("RESSONANCIA") || upper.Contains("RESSONÂNCIA") || upper.Contains("RM -"))
            return "RM";
        if (upper.StartsWith("TC ") || upper.StartsWith("CT ") || upper.Contains("TOMOGRAFIA") || upper.Contains("TOMO ") || upper.Contains("TC -") || upper.Contains("CT -"))
            return "TC";
        if (upper.StartsWith("US ") || upper.Contains("ULTRASSOM") || upper.Contains("ECOGRAFIA") || upper.Contains("DOPPLER"))
            return "US";
        if (upper.StartsWith("RX ") || upper.Contains("RAIO X") || upper.Contains("RADIOGRAFIA"))
            return "RX";
        if (upper.Contains("MAMOGRAFIA") || upper.StartsWith("MMG"))
            return "MMG";
        if (upper.Contains("DENSITOMETRIA"))
            return "DENSITO";

        return "OUTROS";
    }
}
