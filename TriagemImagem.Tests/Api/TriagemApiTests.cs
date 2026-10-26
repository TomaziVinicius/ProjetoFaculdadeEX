using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Xunit;
using Xunit.Abstractions;

namespace TriagemImagem.Tests.Api;

[Collection("API")]
public sealed class TriagemApiTests : ApiTestBase
{
    public TriagemApiTests(ITestOutputHelper output) : base(output) { }

    [Fact]
    public async Task Calcular_dose_TC_deve_retornar_volume_esperado()
    {
        if (!await IsApiReachableAsync()) return;

        var response = await Client.PostAsJsonAsync("api/v1/triagem/calcular-dose", new { modalidade = 1, pesoKg = 80m, fatorMlPorKg = 1.5m });
        var body = await LogResponseAsync("POST /api/v1/triagem/calcular-dose", response);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        using var json = JsonDocument.Parse(body);
        Assert.Equal(120m, json.RootElement.GetProperty("volumeAjustadoMl").GetDecimal());
    }

    [Fact]
    public async Task Obter_triagem_inexistente_deve_retornar_404()
    {
        if (!await IsApiReachableAsync()) return;

        var response = await Client.GetAsync("api/v1/triagem/exame/999999");
        var body = await LogResponseAsync("GET /api/v1/triagem/exame/999999", response);
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Contains("Triagem não encontrada", body);
    }
}
