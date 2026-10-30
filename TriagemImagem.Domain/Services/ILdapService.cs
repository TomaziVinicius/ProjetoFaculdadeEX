namespace TriagemImagem.Domain.Services;

/// <summary>
/// Informações cadastrais e de pertencimento a grupos extraídas do Active Directory
/// </summary>
public class LdapUsuarioInfo
{
    public string Username { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string DistinguishedName { get; set; } = string.Empty;
    public List<string> Groups { get; set; } = new();
    public bool IsInTargetGroup { get; set; }
    public bool IsAdmin { get; set; }
}

/// <summary>
/// Resultado da operação de autenticação no Active Directory
/// </summary>
public class LdapAuthResult
{
    public bool Success { get; set; }
    public string? Error { get; set; }
    public string? ErrorType { get; set; } // 'USER_NOT_FOUND', 'INVALID_CREDENTIALS', 'ACCOUNT_DISABLED', 'GROUP_NOT_MEMBER', 'SERVER_ERROR'
    public LdapUsuarioInfo? User { get; set; }

    public static LdapAuthResult Ok(LdapUsuarioInfo user) => new() { Success = true, User = user };
    public static LdapAuthResult Fail(string error, string? errorType = null) => new() { Success = false, Error = error, ErrorType = errorType };
}

/// <summary>
/// Contrato de serviço para autenticação e consulta de usuários/grupos no Active Directory corporativo
/// </summary>
public interface ILdapService
{
    Task<LdapAuthResult> AutenticarAsync(string username, string password);
    Task<List<LdapUsuarioInfo>> ListarMembrosDoGrupoAsync();
    Task<(bool sucesso, string mensagem, string servidor, string grupo)> TestarConexaoAsync();
}
