namespace TriagemImagem.Domain.Configurations;

public class StorageSettings
{
    public const string SectionName = "StorageSettings";

    /// <summary>
    /// Caminho UNC canônico onde as fichas de triagem são arquivadas.
    /// Exemplo: \\fsa\anexos\IMI\Anexos_Formularios_PROJ
    /// </summary>
    public string DiretorioAnexos { get; set; } = @"\\fsa\anexos\IMI\Anexos_Formularios_PROJ";

    /// <summary>
    /// IP ou Hostname do servidor SMB/CIFS (ex: 10.250.220.210 ou fsa.imi.local).
    /// </summary>
    public string SmbHost { get; set; } = "10.250.220.210";

    /// <summary>
    /// Nome do compartilhamento SMB (ex: anexos).
    /// </summary>
    public string SmbShare { get; set; } = "anexos";

    /// <summary>
    /// Subpasta dentro do compartilhamento (ex: IMI\Anexos_Formularios_PROJ).
    /// </summary>
    public string SmbSubDiretorio { get; set; } = @"IMI\Anexos_Formularios_PROJ";

    /// <summary>
    /// Domínio Active Directory para autenticação SMB.
    /// </summary>
    public string SmbDominio { get; set; } = "IMI";

    /// <summary>
    /// Usuário com permissão de escrita no compartilhamento.
    /// </summary>
    public string SmbUsuario { get; set; } = "laudos.imi";

    /// <summary>
    /// Senha de acesso.
    /// </summary>
    public string SmbSenha { get; set; } = "Mudar@123";
}
