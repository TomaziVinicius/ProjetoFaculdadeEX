namespace TriagemImagem.Domain.Configurations;

public class LdapSettings
{
    public const string SectionName = "LdapSettings";

    public bool Enabled { get; set; } = true;
    public string Server { get; set; } = "10.250.220.200";
    public int Port { get; set; } = 389;
    public string BaseDn { get; set; } = "DC=imi,DC=local";
    public string BindUser { get; set; } = "laudos.imi@imi.local";
    public string BindPassword { get; set; } = "Mudar@123";
    public string TargetGroup { get; set; } = "GR_PROJ_FORMS";
    public string TargetGroupDn { get; set; } = "CN=GR_PROJ_FORMS,OU=Grupos,OU=T.I,DC=imi,DC=local";
    public string AdminGroupDn { get; set; } = "CN=GR_TI_ADMINS,OU=Grupos,OU=T.I,DC=imi,DC=local";
    public int TimeoutSeconds { get; set; } = 6;
}
