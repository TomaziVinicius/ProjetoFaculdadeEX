using System.Net;
using System.Net.Http.Json;
using Xunit;
using Xunit.Abstractions;

namespace TriagemImagem.Tests.Api;

[Collection("API")]
public sealed class AuthApiInputTests : ApiTestBase
{
    public AuthApiInputTests(ITestOutputHelper output) : base(output) { }

    [Fact]
    public async Task Login_com_credenciais_invalidas_retorna_401()
    {
        await GarantirApiDisponivelAsync();
        var response = await Client.PostAsJsonAsync("api/v1/auth/login", TestDataFactory.LoginInvalido());
        var body = await LogResponseAsync("POST /api/v1/auth/login [credenciais inválidas]", response);
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        Assert.Contains("inválidos", body, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Login_sem_campos_obrigatorios_retorna_401()
    {
        await GarantirApiDisponivelAsync();
        var response = await Client.PostAsJsonAsync("api/v1/auth/login", new { login = "", senha = "" });
        await LogResponseAsync("POST /api/v1/auth/login [campos vazios]", response);
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
