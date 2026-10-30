using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Novell.Directory.Ldap;
using TriagemImagem.Domain.Configurations;
using TriagemImagem.Domain.Services;

namespace TriagemImagem.Infrastructure.Services;

public class LdapService : ILdapService
{
    private readonly LdapSettings _settings;
    private readonly ILogger<LdapService> _logger;

    public LdapService(IOptions<LdapSettings> settings, ILogger<LdapService> logger)
    {
        _settings = settings.Value;
        _logger = logger;
    }

    public async Task<LdapAuthResult> AutenticarAsync(string username, string password)
    {
        if (!_settings.Enabled)
            return LdapAuthResult.Fail("A autenticação LDAP está desativada na configuração.", "LDAP_DISABLED");

        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            return LdapAuthResult.Fail("Usuário e senha são obrigatórios.", "EMPTY_CREDENTIALS");

        var cleanUser = username.Trim();
        var samAccount = cleanUser.Contains('@') ? cleanUser.Split('@')[0] : cleanUser;
        var userUpn = cleanUser.Contains('@') ? cleanUser : $"{cleanUser}@imi.local";

        LdapEntry? userEntry = null;

        try
        {
            using var serviceConn = new LdapConnection { SecureSocketLayer = false };
            serviceConn.ConnectionTimeout = _settings.TimeoutSeconds * 1000;
            await serviceConn.ConnectAsync(_settings.Server, _settings.Port);
            await serviceConn.BindAsync(_settings.BindUser, _settings.BindPassword);

            var escapedSam = EscapeLdapFilter(samAccount);
            var escapedUpn = EscapeLdapFilter(userUpn);
            var escapedClean = EscapeLdapFilter(cleanUser);

            var filter = $"(|(sAMAccountName={escapedSam})(userPrincipalName={escapedUpn})(userPrincipalName={escapedClean}))";
            var attributes = new[]
            {
                "displayName", "givenName", "sn", "mail", "sAMAccountName",
                "userPrincipalName", "memberOf", "distinguishedName", "userAccountControl"
            };

            var searchResults = await serviceConn.SearchAsync(
                _settings.BaseDn,
                LdapConnection.ScopeSub,
                filter,
                attributes,
                false
            );

            await foreach (var entry in searchResults)
            {
                userEntry = entry;
                break;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar usuário no Active Directory ({Server}:{Port})", _settings.Server, _settings.Port);
            return LdapAuthResult.Fail($"Erro de comunicação com o controlador de domínio (AD): {ex.Message}", "SERVER_ERROR");
        }

        if (userEntry == null)
            return LdapAuthResult.Fail("Usuário não localizado no domínio corporativo (Active Directory).", "USER_NOT_FOUND");

        // 2. Verificar se conta está desativada (UAC bit 2)
        var uacStr = ObterAtributoString(userEntry, "userAccountControl");
        if (int.TryParse(uacStr, out var uac) && (uac & 2) == 2)
        {
            return LdapAuthResult.Fail("Esta conta de rede está desativada no Active Directory.", "ACCOUNT_DISABLED");
        }

        // 3. Autenticar com a senha do usuário
        var bindTarget = ObterAtributoString(userEntry, "userPrincipalName");
        if (string.IsNullOrWhiteSpace(bindTarget)) bindTarget = userEntry.Dn ?? userUpn;

        try
        {
            using var userConn = new LdapConnection { SecureSocketLayer = false };
            userConn.ConnectionTimeout = _settings.TimeoutSeconds * 1000;
            await userConn.ConnectAsync(_settings.Server, _settings.Port);
            await userConn.BindAsync(bindTarget, password);
        }
        catch (LdapException lex)
        {
            _logger.LogWarning(lex, "Falha de credenciais no AD para usuário '{SamAccount}'", samAccount);
            return LdapAuthResult.Fail("Senha incorreta no domínio corporativo.", "INVALID_CREDENTIALS");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro inesperado ao validar credenciais no AD para '{SamAccount}'", samAccount);
            return LdapAuthResult.Fail($"Erro ao validar credenciais no AD: {ex.Message}", "SERVER_ERROR");
        }

        // 4. Verificação estrita de pertinência ao grupo obrigatório (GR_PROJ_FORMS)
        var isMember = await VerificarPertinenciaAoGrupoAsync(userEntry, _settings.TargetGroupDn, _settings.TargetGroup);
        if (!isMember)
        {
            _logger.LogWarning("Acesso negado: Usuário '{SamAccount}' autenticou no AD mas NÃO pertence ao grupo '{TargetGroup}'", samAccount, _settings.TargetGroup);
            return LdapAuthResult.Fail(
                $"Acesso negado: seu usuário de rede não pertence ao grupo corporativo ({_settings.TargetGroup}). Solicite ingresso no grupo à equipe de T.I.",
                "GROUP_NOT_MEMBER"
            );
        }

        // 5. Verificar se é Admin (ex: GR_TI_ADMINS ou GR_TI)
        var isAdmin = await VerificarPertinenciaAoGrupoAsync(userEntry, _settings.AdminGroupDn, "GR_TI_ADMINS")
                   || await VerificarPertinenciaAoGrupoAsync(userEntry, "", "GR_TI");

        // 6. Montar dados
        var displayName = ObterAtributoString(userEntry, "displayName");
        if (string.IsNullOrWhiteSpace(displayName))
        {
            var given = ObterAtributoString(userEntry, "givenName");
            var sn = ObterAtributoString(userEntry, "sn");
            displayName = $"{given} {sn}".Trim();
            if (string.IsNullOrWhiteSpace(displayName)) displayName = samAccount;
        }

        var mail = ObterAtributoString(userEntry, "mail");
        if (string.IsNullOrWhiteSpace(mail)) mail = userUpn;

        var groups = ObterAtributosArray(userEntry, "memberOf");

        var userInfo = new LdapUsuarioInfo
        {
            Username = samAccount,
            DisplayName = displayName,
            Email = mail,
            DistinguishedName = userEntry.Dn ?? "",
            Groups = groups,
            IsInTargetGroup = true,
            IsAdmin = isAdmin
        };

        return LdapAuthResult.Ok(userInfo);
    }

    public async Task<List<LdapUsuarioInfo>> ListarMembrosDoGrupoAsync()
    {
        var membros = new List<LdapUsuarioInfo>();
        if (!_settings.Enabled) return membros;

        try
        {
            using var conn = new LdapConnection { SecureSocketLayer = false };
            conn.ConnectionTimeout = _settings.TimeoutSeconds * 1000;
            await conn.ConnectAsync(_settings.Server, _settings.Port);
            await conn.BindAsync(_settings.BindUser, _settings.BindPassword);

            var groupDn = !string.IsNullOrWhiteSpace(_settings.TargetGroupDn)
                ? _settings.TargetGroupDn
                : $"CN={_settings.TargetGroup},OU=Grupos,OU=T.I,{_settings.BaseDn}";

            var filter = $"(&(objectClass=user)(memberOf:1.2.840.113556.1.4.1941:={EscapeLdapFilter(groupDn)}))";
            var attributes = new[] { "displayName", "givenName", "sn", "mail", "sAMAccountName", "distinguishedName" };

            var results = await conn.SearchAsync(_settings.BaseDn, LdapConnection.ScopeSub, filter, attributes, false);

            await foreach (var entry in results)
            {
                var sam = ObterAtributoString(entry, "sAMAccountName");
                if (string.IsNullOrWhiteSpace(sam)) continue;

                var display = ObterAtributoString(entry, "displayName");
                if (string.IsNullOrWhiteSpace(display))
                {
                    var gn = ObterAtributoString(entry, "givenName");
                    var sn = ObterAtributoString(entry, "sn");
                    display = $"{gn} {sn}".Trim();
                    if (string.IsNullOrWhiteSpace(display)) display = sam;
                }

                var email = ObterAtributoString(entry, "mail");
                if (string.IsNullOrWhiteSpace(email)) email = $"{sam}@imi.local";

                membros.Add(new LdapUsuarioInfo
                {
                    Username = sam,
                    DisplayName = display,
                    Email = email,
                    DistinguishedName = entry.Dn,
                    IsInTargetGroup = true
                });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao listar membros do grupo LDAP '{Group}'", _settings.TargetGroup);
        }

        return membros;
    }

    public async Task<(bool sucesso, string mensagem, string servidor, string grupo)> TestarConexaoAsync()
    {
        try
        {
            using var conn = new LdapConnection { SecureSocketLayer = false };
            conn.ConnectionTimeout = _settings.TimeoutSeconds * 1000;
            await conn.ConnectAsync(_settings.Server, _settings.Port);
            await conn.BindAsync(_settings.BindUser, _settings.BindPassword);

            var groupDn = !string.IsNullOrWhiteSpace(_settings.TargetGroupDn)
                ? _settings.TargetGroupDn
                : $"CN={_settings.TargetGroup},OU=Grupos,OU=T.I,{_settings.BaseDn}";

            var groupSearch = await conn.SearchAsync(
                _settings.BaseDn,
                LdapConnection.ScopeSub,
                $"(distinguishedName={EscapeLdapFilter(groupDn)})",
                new[] { "sAMAccountName", "member" },
                false
            );

            var groupFound = false;
            await foreach (var _ in groupSearch)
            {
                groupFound = true;
                break;
            }

            return (
                sucesso: true,
                mensagem: groupFound 
                    ? $"Conexão LDAP bem-sucedida com '{_settings.Server}:{_settings.Port}'. Grupo '{_settings.TargetGroup}' localizado no diretório."
                    : $"Conexão LDAP bem-sucedida com '{_settings.Server}:{_settings.Port}', porém o grupo '{groupDn}' não foi encontrado.",
                servidor: $"{_settings.Server}:{_settings.Port}",
                grupo: _settings.TargetGroup
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Falha no teste de conexão LDAP com '{Server}'", _settings.Server);
            return (
                sucesso: false,
                mensagem: $"Falha ao conectar no Active Directory ({_settings.Server}:{_settings.Port}): {ex.Message}",
                servidor: $"{_settings.Server}:{_settings.Port}",
                grupo: _settings.TargetGroup
            );
        }
    }

    private async Task<bool> VerificarPertinenciaAoGrupoAsync(LdapEntry userEntry, string targetGroupDn, string targetGroupName)
    {
        if (string.IsNullOrWhiteSpace(targetGroupDn) && string.IsNullOrWhiteSpace(targetGroupName))
            return false;

        var rawTarget = (targetGroupDn ?? "").Trim().ToLowerInvariant();
        var rawName = (targetGroupName ?? "").Trim().ToLowerInvariant();

        // 1. Verificação direta nos atributos memberOf do objeto do usuário
        var directGroups = ObterAtributosArray(userEntry, "memberOf");
        foreach (var groupVal in directGroups)
        {
            var gLower = (groupVal ?? "").Trim().ToLowerInvariant();
            if (!string.IsNullOrWhiteSpace(rawTarget) && gLower == rawTarget) return true;
            if (!string.IsNullOrWhiteSpace(rawName) && gLower.Contains($"cn={rawName},")) return true;
            if (!string.IsNullOrWhiteSpace(rawName) && gLower.StartsWith($"cn={rawName}")) return true;
        }

        // 2. Verificação recursiva via regra matching rule LDAP_MATCHING_RULE_IN_CHAIN (Active Directory)
        if (!string.IsNullOrWhiteSpace(userEntry.Dn) && !string.IsNullOrWhiteSpace(targetGroupDn))
        {
            try
            {
                using var checkConn = new LdapConnection { SecureSocketLayer = false };
                checkConn.ConnectionTimeout = _settings.TimeoutSeconds * 1000;
                await checkConn.ConnectAsync(_settings.Server, _settings.Port);
                await checkConn.BindAsync(_settings.BindUser, _settings.BindPassword);

                var filter = $"(&(distinguishedName={EscapeLdapFilter(userEntry.Dn)})(memberOf:1.2.840.113556.1.4.1941:={EscapeLdapFilter(targetGroupDn)}))";
                var results = await checkConn.SearchAsync(_settings.BaseDn, LdapConnection.ScopeSub, filter, new[] { "sAMAccountName" }, false);

                await foreach (var _ in results)
                {
                    return true;
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Aviso ao realizar busca recursiva de grupo LDAP para {UserDn}", userEntry.Dn);
            }
        }

        return false;
    }

    private static string ObterAtributoString(LdapEntry entry, string attrName)
    {
        try
        {
            var attr = entry.GetAttributeSet()?.GetAttribute(attrName);
            return attr?.StringValue ?? string.Empty;
        }
        catch
        {
            return string.Empty;
        }
    }

    private static List<string> ObterAtributosArray(LdapEntry entry, string attrName)
    {
        var list = new List<string>();
        try
        {
            var attr = entry.GetAttributeSet()?.GetAttribute(attrName);
            if (attr != null)
            {
                var values = attr.StringValueArray;
                if (values != null)
                {
                    foreach (var v in values)
                    {
                        if (!string.IsNullOrWhiteSpace(v)) list.Add(v);
                    }
                }
            }
        }
        catch
        {
            // Retorna lista vazia em caso de ausência
        }
        return list;
    }

    private static string EscapeLdapFilter(string input)
    {
        if (string.IsNullOrEmpty(input)) return string.Empty;

        return input
            .Replace(@"\", @"\5c")
            .Replace("*", @"\2a")
            .Replace("(", @"\28")
            .Replace(")", @"\29")
            .Replace("\0", @"\00");
    }
}
