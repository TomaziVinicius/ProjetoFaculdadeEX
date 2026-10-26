using System.Net;
using Xunit;
using Xunit.Abstractions;

namespace TriagemImagem.Tests.Api;

[Collection("API")]
public sealed class AuditoriaApiTests : ApiTestBase
{
    public AuditoriaApiTests(ITestOutputHelper output) : base(output) { }

    [Fact]
    public async Task Obter_auditoria_deve_retornar_200_e_lista_json()
    {
        var response = await Client.GetAsync("api/v1/auditoria?limite=10");
        var body = await LogResponseAsync("GET /api/v1/auditoria?limite=10", response);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.StartsWith("[", body.Trim());
    }
}
