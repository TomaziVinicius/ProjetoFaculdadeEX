using System.Net;
using System.Net.Http.Json;
using Xunit;
using Xunit.Abstractions;

namespace TriagemImagem.Tests.Api;

[Collection("API")]
public sealed class WorklistApiInputTests : ApiTestBase
{
    public WorklistApiInputTests(ITestOutputHelper output) : base(output) { }

    [Fact]
    public async Task Consulta_com_filtro_valido_retorna_200()
    {
        await GarantirApiDisponivelAsync();
        var response = await Client.GetAsync("api/v1/worklist?modalidade=1");
        await LogResponseAsync("GET /api/v1/worklist?modalidade=1", response);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Consulta_com_modalidade_invalida_retorna_400()
    {
        await GarantirApiDisponivelAsync();
        var response = await Client.GetAsync("api/v1/worklist?modalidade=99");
        await LogResponseAsync("GET /api/v1/worklist?modalidade=99", response);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Chamada_sem_operador_retorna_400_sem_alterar_exame()
    {
        await GarantirApiDisponivelAsync();
        var response = await Client.PostAsJsonAsync("api/v1/worklist/1/chamar", new { usuarioId = 0 });
        await LogResponseAsync("POST /api/v1/worklist/1/chamar [operador inválido]", response);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
