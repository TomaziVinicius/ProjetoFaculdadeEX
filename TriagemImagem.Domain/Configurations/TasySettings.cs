namespace TriagemImagem.Domain.Configurations;

public sealed class TasySettings
{
    public const string SectionName = "TasySettings";

    public string Host { get; set; } = "192.1.1.12";
    public int Port { get; set; } = 1521;
    public string ServiceName { get; set; } = "tasytst";
    public string User { get; set; } = "IMI_FORMULARIO";
    public string Password { get; set; } = "(q)qiS6*lupKu)}*";
    public int ConnectionTimeoutSeconds { get; set; } = 10;
    public string ProcedimentosView { get; set; } = "TASY.IMI_EXAMES_PROC_INTERNO_V";
    public string ExamesDiaView { get; set; } = "TASY.IMI_EXAMES_DIA_FORM_V";

    public string BuildConnectionString()
    {
        return $"User Id={User};Password={Password};Data Source=(DESCRIPTION=(ADDRESS=(PROTOCOL=TCP)(HOST={Host})(PORT={Port}))(CONNECT_DATA=(SERVICE_NAME={ServiceName})));Connection Timeout={ConnectionTimeoutSeconds};";
    }
}
