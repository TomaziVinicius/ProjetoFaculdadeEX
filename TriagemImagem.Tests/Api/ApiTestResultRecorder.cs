using System.Text.Json;

namespace TriagemImagem.Tests.Api;

public static class ApiTestResultRecorder
{
    private static readonly SemaphoreSlim Lock = new(1, 1);

    public static async Task RegistrarAsync(string request, HttpResponseMessage response, string body)
    {
        var pasta = Path.Combine(Directory.GetCurrentDirectory(), "TestResults");
        Directory.CreateDirectory(pasta);
        var registro = new { dataHoraUtc = DateTime.UtcNow, requisicao = request, statusHttp = (int)response.StatusCode, status = response.StatusCode.ToString(), corpoResposta = body.Length <= 4000 ? body : body[..4000] + " ...[resposta truncada]" };
        await Lock.WaitAsync();
        try { await File.AppendAllTextAsync(Path.Combine(pasta, "api-input-results.jsonl"), JsonSerializer.Serialize(registro) + Environment.NewLine); }
        finally { Lock.Release(); }
    }
}
