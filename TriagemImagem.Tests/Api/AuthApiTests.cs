using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Xunit;
using Xunit.Abstractions;

namespace TriagemImagem.Tests.Api;

[Collection("API")]
public sealed class AuthApiTests : ApiTestBase
{
    public AuthApiTests(ITestOutputHelper output) : base(output) { }

    [Fact]
    public async Task Login_com_credenciais_invalidas_deve_retornar_401()
    {
        var response = await Client.PostAsJsonAsync("api/v1/auth/login", new { login = "usuario.invalido", senha = "senha-invalida" });
        var body = await LogResponseAsync("POST /api/v1/auth/login", response);
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        using var json = JsonDocument.Parse(body);
        Assert.False(json.RootElement.GetProperty("success").GetBoolean());
    }
}
