using System.Net;
using System.Net.Http.Json;
using Xunit;
using Xunit.Abstractions;

namespace TriagemImagem.Tests.Api;

[Collection("API")]
public sealed class WorklistApiTests : ApiTestBase
{
    public WorklistApiTests(ITestOutputHelper output) : base(output) { }

    [Fact]
    public async Task Obter_worklist_deve_retornar_200_e_lista_json()
    {
        if (!await IsApiReachableAsync()) return;

        var response = await Client.GetAsync("api/v1/worklist");
        var body = await LogResponseAsync("GET /api/v1/worklist", response);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.StartsWith("[", body.Trim());
    }

    [Fact]
    public async Task Obter_exame_inexistente_deve_retornar_404()
    {
        if (!await IsApiReachableAsync()) return;

        var response = await Client.GetAsync("api/v1/worklist/999999");
        var body = await LogResponseAsync("GET /api/v1/worklist/999999", response);
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Contains("Exame não encontrado", body);
    }

    [Fact]
    public async Task Obter_exame_com_id_invalido_deve_retornar_400()
    {
        if (!await IsApiReachableAsync()) return;

        var response = await Client.GetAsync("api/v1/worklist/0");
        var body = await LogResponseAsync("GET /api/v1/worklist/0", response);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Contains("maior que zero", body);
    }

    [Fact]
    public async Task Listar_worklist_com_modalidade_invalida_deve_retornar_400()
    {
        if (!await IsApiReachableAsync()) return;

        var response = await Client.GetAsync("api/v1/worklist?modalidade=99");
        var body = await LogResponseAsync("GET /api/v1/worklist?modalidade=99", response);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.True(body.Contains("Modalidade", StringComparison.OrdinalIgnoreCase) || body.Contains("invalid", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task Chamar_paciente_sem_usuario_valido_deve_retornar_400()
    {
        if (!await IsApiReachableAsync()) return;

        var response = await Client.PostAsJsonAsync("api/v1/worklist/1/chamar", new { usuarioId = 0 });
        var body = await LogResponseAsync("POST /api/v1/worklist/1/chamar", response);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Contains("operador responsável", body);
    }

    [Fact]
    public async Task Chamar_paciente_exame_inexistente_deve_retornar_404()
    {
        if (!await IsApiReachableAsync()) return;

        var response = await Client.PostAsJsonAsync("api/v1/worklist/999999/chamar", new { usuarioId = 1 });
        var body = await LogResponseAsync("POST /api/v1/worklist/999999/chamar", response);
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Contains("Exame não encontrado", body);
    }
}
