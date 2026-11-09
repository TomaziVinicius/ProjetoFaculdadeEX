using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SMBLibrary;
using SMBLibrary.Client;
using TriagemImagem.Application.Interfaces;
using TriagemImagem.Domain.Configurations;

namespace TriagemImagem.Infrastructure.Services;

public class FileStorageService : IFileStorageService
{
    private readonly StorageSettings _settings;
    private readonly ILogger<FileStorageService> _logger;

    public FileStorageService(
        IOptions<StorageSettings> settings,
        ILogger<FileStorageService> logger)
    {
        _settings = settings.Value ?? new StorageSettings();
        _logger = logger;
    }

    public async Task<string> SalvarArquivoAsync(string nomeArquivo, string conteudoHtml)
    {
        var conteudoBytes = Encoding.UTF8.GetBytes(conteudoHtml);
        var dirFormatado = _settings.DiretorioAnexos.TrimEnd('\\', '/').Replace('/', '\\');
        var caminhoCanonico = $"{dirFormatado}\\{nomeArquivo}";

        // 1. Tentativa de escrita direta via sistema operacional (Nativa para Windows ou shares montados)
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            try
            {
                if (!Directory.Exists(_settings.DiretorioAnexos))
                {
                    Directory.CreateDirectory(_settings.DiretorioAnexos);
                }

                var caminhoFisico = Path.Combine(_settings.DiretorioAnexos, nomeArquivo);
                await File.WriteAllBytesAsync(caminhoFisico, conteudoBytes);
                _logger.LogInformation("Arquivo salvo com sucesso via FileSystem: '{Caminho}'", caminhoFisico);
                return caminhoCanonico;
            }
            catch (Exception exFs)
            {
                _logger.LogWarning(exFs, "Escrita direta via FileSystem falhou em '{Diretorio}'. Tentando via cliente SMB...", _settings.DiretorioAnexos);
            }
        }

        // 2. Tentativa de escrita via protocolo de rede SMB (Multiplataforma, para Linux/CloudPanel)
        try
        {
            var smbSucesso = SalvarViaSmb(nomeArquivo, conteudoBytes);
            if (smbSucesso)
            {
                _logger.LogInformation("Arquivo salvo com sucesso via protocolo SMB em '{Caminho}'", caminhoCanonico);
                return caminhoCanonico;
            }
        }
        catch (Exception exSmb)
        {
            _logger.LogWarning(exSmb, "Falha ao gravar arquivo via protocolo SMB no servidor '{Host}'.", _settings.SmbHost);
        }

        // 3. Fallback de contingência local para garantir que a triagem do paciente nunca seja perdida
        try
        {
            var pastaFallback = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                "TriagensDiagnosticoImagem");

            if (string.IsNullOrWhiteSpace(pastaFallback) || !Directory.Exists(pastaFallback))
            {
                Directory.CreateDirectory(pastaFallback);
            }

            var caminhoFallback = Path.Combine(pastaFallback, nomeArquivo);
            await File.WriteAllBytesAsync(caminhoFallback, conteudoBytes);
            _logger.LogWarning("Arquivo arquivado em contingência local: '{CaminhoFallback}'.", caminhoFallback);
            return caminhoFallback;
        }
        catch (Exception exFallback)
        {
            _logger.LogError(exFallback, "Falha crítica ao arquivar relatório. Nenhum meio de armazenamento respondeu.");
            return caminhoCanonico;
        }
    }

    private bool SalvarViaSmb(string nomeArquivo, byte[] conteudo)
    {
        var client = new SMB2Client();
        try
        {
            // Resolve endereço IP do host SMB
            IPAddress? targetIp;
            if (!IPAddress.TryParse(_settings.SmbHost, out targetIp))
            {
                var entry = Dns.GetHostEntry(_settings.SmbHost);
                targetIp = entry.AddressList.FirstOrDefault(a => a.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork);
            }

            if (targetIp == null)
            {
                _logger.LogWarning("Não foi possível resolver o IP do servidor SMB '{Host}'.", _settings.SmbHost);
                return false;
            }

            bool conectado = client.Connect(targetIp, SMBTransportType.DirectTCPTransport);
            if (!conectado)
            {
                _logger.LogWarning("Falha ao abrir conexão TCP 445 com o servidor SMB '{Ip}'.", targetIp);
                return false;
            }

            var loginStatus = client.Login(_settings.SmbDominio, _settings.SmbUsuario, _settings.SmbSenha);
            if (loginStatus != NTStatus.STATUS_SUCCESS)
            {
                _logger.LogWarning("Falha na autenticação SMB com o usuário '{User}': {Status}", _settings.SmbUsuario, loginStatus);
                return false;
            }

            var fileStore = client.TreeConnect(_settings.SmbShare, out var status);
            if (status != NTStatus.STATUS_SUCCESS)
            {
                _logger.LogWarning("Falha ao conectar no compartilhamento SMB '{Share}': {Status}", _settings.SmbShare, status);
                return false;
            }

            // Normaliza o caminho relativo dentro do share
            var subDiretorio = _settings.SmbSubDiretorio.Trim().Trim('\\', '/');
            var caminhoRelativo = string.IsNullOrEmpty(subDiretorio)
                ? nomeArquivo
                : $@"{subDiretorio}\{nomeArquivo}";

            object fileHandle;
            FileStatus fileStatus;
            status = fileStore.CreateFile(
                out fileHandle,
                out fileStatus,
                caminhoRelativo,
                AccessMask.GENERIC_WRITE | AccessMask.SYNCHRONIZE,
                SMBLibrary.FileAttributes.Normal,
                ShareAccess.Read | ShareAccess.Write,
                CreateDisposition.FILE_OVERWRITE_IF,
                CreateOptions.FILE_NON_DIRECTORY_FILE | CreateOptions.FILE_SYNCHRONOUS_IO_NONALERT,
                null);

            if (status != NTStatus.STATUS_SUCCESS)
            {
                _logger.LogWarning("Falha ao criar arquivo no SMB ('{Path}'): {Status}", caminhoRelativo, status);
                fileStore.Disconnect();
                client.Logoff();
                return false;
            }

            status = fileStore.WriteFile(out int bytesWritten, fileHandle, 0, conteudo);
            fileStore.CloseFile(fileHandle);
            fileStore.Disconnect();
            client.Logoff();

            return status == NTStatus.STATUS_SUCCESS;
        }
        finally
        {
            client.Disconnect();
        }
    }
}
