using System.Net;
using System.Text;
using SMBLibrary;
using SMBLibrary.Client;
using Xunit;
using Xunit.Abstractions;

namespace TriagemImagem.Tests.Services;

public class SmbStorageTest
{
    private readonly ITestOutputHelper _output;

    public SmbStorageTest(ITestOutputHelper output)
    {
        _output = output;
    }

    [Fact]
    [Trait("Category", "Integration")]
    public void TestarEscritaArquivoSmbFsa()
    {
        var client = new SMB2Client();
        try
        {
            var ip = IPAddress.Parse("10.250.220.210");
            if (!client.Connect(ip, SMBTransportType.DirectTCPTransport))
            {
                _output.WriteLine("Host SMB inacessível neste ambiente, teste pulado.");
                return;
            }

            var loginStatus = client.Login("IMI", "laudos.imi", "Mudar@123");
            Assert.True(loginStatus == NTStatus.STATUS_SUCCESS, $"Login falhou: {loginStatus}");

            var fileStore = client.TreeConnect("anexos", out var status);
            Assert.True(status == NTStatus.STATUS_SUCCESS, $"TreeConnect falhou: {status}");

            // Caminho relativo ao share 'anexos': IMI\Anexos_Formularios_PROJ\teste_smb.txt
            var caminhoRelativo = @"IMI\Anexos_Formularios_PROJ\teste_smb.txt";
            var conteudo = Encoding.UTF8.GetBytes("Teste de escrita via SMBLibrary .NET no servidor fsa");

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

            Assert.True(status == NTStatus.STATUS_SUCCESS, $"CreateFile falhou: {status}");

            int bytesWritten;
            status = fileStore.WriteFile(out bytesWritten, fileHandle, 0, conteudo);
            Assert.True(status == NTStatus.STATUS_SUCCESS, $"WriteFile falhou: {status}");
            Assert.Equal(conteudo.Length, bytesWritten);

            status = fileStore.CloseFile(fileHandle);
            Assert.True(status == NTStatus.STATUS_SUCCESS, $"CloseFile falhou: {status}");

            // Limpa o arquivo de teste
            status = fileStore.CreateFile(
                out fileHandle,
                out fileStatus,
                caminhoRelativo,
                AccessMask.DELETE | AccessMask.SYNCHRONIZE,
                SMBLibrary.FileAttributes.Normal,
                ShareAccess.None,
                CreateDisposition.FILE_OPEN,
                CreateOptions.FILE_NON_DIRECTORY_FILE | CreateOptions.FILE_DELETE_ON_CLOSE,
                null);

            if (status == NTStatus.STATUS_SUCCESS)
            {
                fileStore.CloseFile(fileHandle);
            }

            fileStore.Disconnect();
            client.Logoff();
            _output.WriteLine("Escrita e exclusão de arquivo via SMB concluída com 100% de sucesso!");
        }
        finally
        {
            client.Disconnect();
        }
    }
}
