using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TriagemImagem.Application.DTOs;
using TriagemImagem.Application.Interfaces;
using TriagemImagem.Domain.Configurations;
using TriagemImagem.Domain.Entities;
using TriagemImagem.Domain.Enums;
using TriagemImagem.Domain.Exceptions;
using TriagemImagem.Domain.Repositories;
using TriagemImagem.Domain.Services;

namespace TriagemImagem.Application.Services;

public class AuthAppService : IAuthAppService
{
    private readonly IUsuarioRepository _usuarioRepository;
    private readonly IAuditoriaRepository _auditoriaRepository;
    private readonly ILdapService _ldapService;
    private readonly LdapSettings _ldapSettings;
    private readonly ILogger<AuthAppService> _logger;

    public AuthAppService(
        IUsuarioRepository usuarioRepository, 
        IAuditoriaRepository auditoriaRepository,
        ILdapService ldapService,
        IOptions<LdapSettings> ldapSettings,
        ILogger<AuthAppService> logger)
    {
        _usuarioRepository = usuarioRepository;
        _auditoriaRepository = auditoriaRepository;
        _ldapService = ldapService;
        _ldapSettings = ldapSettings.Value;
        _logger = logger;
    }

    public async Task<UsuarioAutenticadoDto?> AutenticarAsync(LoginRequestDto request)
    {
        if (string.IsNullOrWhiteSpace(request.Login) || string.IsNullOrWhiteSpace(request.Senha))
            return null;

        var cleanLogin = request.Login.Trim();
        var samAccount = cleanLogin.Contains('@') ? cleanLogin.Split('@')[0] : cleanLogin;

        // 1. Se o LDAP estiver habilitado, tenta autenticar no Active Directory primeiro
        if (_ldapSettings.Enabled)
        {
            var ldapResult = await _ldapService.AutenticarAsync(cleanLogin, request.Senha);

            if (ldapResult.Success && ldapResult.User != null)
            {
                // Usuário autenticado com sucesso e validado no grupo corporativo!
                var usuario = await SincronizarUsuarioLdapAsync(ldapResult.User);

                await _auditoriaRepository.RegistrarLogAsync(new AuditoriaLog
                {
                    UsuarioId = usuario.Id,
                    TipoEvento = "LOGIN_LDAP_SUCESSO",
                    Detalhes = $"Usuário corporativo '{usuario.Login}' ({usuario.Perfil}) autenticado via LDAP no grupo {_ldapSettings.TargetGroup}."
                });

                return new UsuarioAutenticadoDto
                {
                    Id = usuario.Id,
                    Nome = usuario.Nome,
                    Login = usuario.Login,
                    Perfil = usuario.Perfil,
                    AutenticadoEm = DateTime.UtcNow
                };
            }

            // Se o usuário foi autenticado mas NÃO pertence ao grupo obrigatório, bloqueia o acesso com mensagem explícita
            if (ldapResult.ErrorType == "GROUP_NOT_MEMBER")
            {
                var usuarioExistente = await _usuarioRepository.ObterPorLoginAsync(samAccount);
                await _auditoriaRepository.RegistrarLogAsync(new AuditoriaLog
                {
                    UsuarioId = usuarioExistente?.Id ?? 1,
                    TipoEvento = "LOGIN_LDAP_NEGADO_GRUPO",
                    Detalhes = $"Acesso negado para '{samAccount}': não pertence ao grupo '{_ldapSettings.TargetGroup}'."
                });

                throw new DomainException(ldapResult.Error ?? $"Acesso negado: seu usuário não pertence ao grupo corporativo {_ldapSettings.TargetGroup}.");
            }

            // Se a conta estiver desativada no AD
            if (ldapResult.ErrorType == "ACCOUNT_DISABLED")
            {
                throw new DomainException("Esta conta de rede está desativada no Active Directory.");
            }
        }

        // 2. Fallback: Autenticação no Banco de Dados Local (para admin de contingência e contas locais)
        var usuarioLocal = await _usuarioRepository.ObterPorLoginAsync(samAccount);

        if (usuarioLocal != null && usuarioLocal.StatusUsuario && !string.IsNullOrEmpty(usuarioLocal.SenhaUsuario))
        {
            if (usuarioLocal.SenhaUsuario == request.Senha)
            {
                await _auditoriaRepository.RegistrarLogAsync(new AuditoriaLog
                {
                    UsuarioId = usuarioLocal.Id,
                    TipoEvento = "LOGIN_LOCAL_SUCESSO",
                    Detalhes = $"Usuário local '{usuarioLocal.Login}' ({usuarioLocal.Perfil}) autenticado no posto de triagem."
                });

                return new UsuarioAutenticadoDto
                {
                    Id = usuarioLocal.Id,
                    Nome = usuarioLocal.Nome,
                    Login = usuarioLocal.Login,
                    Perfil = usuarioLocal.Perfil,
                    AutenticadoEm = DateTime.UtcNow
                };
            }
        }

        // 3. Falha de autenticação (não encontrado ou senha incorreta)
        await _auditoriaRepository.RegistrarLogAsync(new AuditoriaLog
        {
            UsuarioId = usuarioLocal?.Id ?? 1,
            TipoEvento = "LOGIN_FALHA",
            Detalhes = $"Tentativa de login incorreta para o usuário '{request.Login}'."
        });

        return null;
    }

    private async Task<Usuario> SincronizarUsuarioLdapAsync(LdapUsuarioInfo ldapUser)
    {
        var usuarioExistente = await _usuarioRepository.ObterPorLoginAsync(ldapUser.Username);

        if (usuarioExistente == null)
        {
            // Just-In-Time (JIT) Provisioning: cadastra o novo usuário do AD localmente
            var novoPerfil = ldapUser.IsAdmin ? PerfilUsuario.ADMIN : PerfilUsuario.BIOMEDICO;

            var novoUsuario = new Usuario
            {
                Nome = ldapUser.DisplayName,
                Login = ldapUser.Username.ToLowerInvariant(),
                SenhaUsuario = "", // Não salva senha para usuários do Active Directory
                Perfil = novoPerfil,
                StatusUsuario = true,
                CriadoEm = DateTime.UtcNow
            };

            await _usuarioRepository.AdicionarAsync(novoUsuario);
            await _usuarioRepository.SalvarAlteracoesAsync();
            return novoUsuario;
        }

        // Atualiza os dados cadastrais vindos do AD
        var alterado = false;
        if (!string.IsNullOrWhiteSpace(ldapUser.DisplayName) && usuarioExistente.Nome != ldapUser.DisplayName)
        {
            usuarioExistente.Nome = ldapUser.DisplayName;
            alterado = true;
        }

        if (ldapUser.IsAdmin && usuarioExistente.Perfil != PerfilUsuario.ADMIN)
        {
            usuarioExistente.Perfil = PerfilUsuario.ADMIN;
            alterado = true;
        }

        if (!usuarioExistente.StatusUsuario)
        {
            usuarioExistente.StatusUsuario = true;
            alterado = true;
        }

        if (alterado)
        {
            await _usuarioRepository.AtualizarAsync(usuarioExistente);
            await _usuarioRepository.SalvarAlteracoesAsync();
        }

        return usuarioExistente;
    }
}
