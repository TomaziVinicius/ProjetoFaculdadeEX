using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Xunit;
using Xunit.Abstractions;

namespace TriagemImagem.Tests.Api;

[Collection("API")]
public sealed class ApiSmokeTests
{
    private readonly ITestOutputHelper _output;
    private readonly HttpClient _client;

    public ApiSmokeTests(ITestOutputHelper output)
    {
        _output = output;
        var baseUrl = Environment.GetEnvironmentVariable("API_BASE_URL")
            ?? "http://localhost:5000";

        _client = new HttpClient
        {
            BaseAddress = new Uri(baseUrl.TrimEnd('/') + "/")
        };
    }

    [Fact]
    public async Task Swagger_deve_estar_disponivel()
    {
        var response = await _client.GetAsync("swagger/index.html");
        var body = await response.Content.ReadAsStringAsync();

        Log("GET /swagger/index.html", response, body);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("Swagger", body, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Calculo_de_dose_TC_deve_retornar_200()
    {
        var response = await _client.PostAsJsonAsync(
            "api/v1/triagem/calcular-dose",
            new { modalidade = 1, pesoKg = 80m, fatorMlPorKg = 1.5m });

        var body = await response.Content.ReadAsStringAsync();

        Log("POST /api/v1/triagem/calcular-dose", response, body);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        using var json = JsonDocument.Parse(body);
        Assert.Equal(
            120m,
            json.RootElement.GetProperty("volumeAjustadoMl").GetDecimal());
    }

    [Fact]
    public async Task Worklist_deve_retornar_200()
    {
        var response = await _client.GetAsync("api/v1/worklist");
        var body = await response.Content.ReadAsStringAsync();

        Log("GET /api/v1/worklist", response, body);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.StartsWith("[", body.Trim());
    }

    [Fact]
    public async Task Triagem_inexistente_deve_retornar_404()
    {
        var response = await _client.GetAsync("api/v1/triagem/exame/999999");
        var body = await response.Content.ReadAsStringAsync();

        Log("GET /api/v1/triagem/exame/999999", response, body);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Contains("Triagem não encontrada", body);
    }

    private void Log(string request, HttpResponseMessage response, string body)
    {
        _output.WriteLine($"{request} => {(int)response.StatusCode} ({response.StatusCode})");
        _output.WriteLine(body);
    }
}