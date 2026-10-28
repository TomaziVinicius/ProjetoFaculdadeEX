using System.Net;
using Xunit;
using Xunit.Abstractions;

namespace TriagemImagem.Tests.Api;

[Collection("API")]
public sealed class AuditoriaApiInputTests : ApiTestBase
{
    public AuditoriaApiInputTests(ITestOutputHelper output) : base(output) { }

    [Theory]
    [InlineData(1)]
    [InlineData(10)]
    [InlineData(999)]
    public async Task Consulta_de_auditoria_com_limites_de_entrada_retorna_200(int limite)
    {
        await GarantirApiDisponivelAsync();
        var response = await Client.GetAsync($"api/v1/auditoria?limite={limite}");
        await LogResponseAsync($"GET /api/v1/auditoria?limite={limite}", response);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}
