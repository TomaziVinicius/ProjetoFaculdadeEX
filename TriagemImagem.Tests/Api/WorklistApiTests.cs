using System.Net;
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
}
