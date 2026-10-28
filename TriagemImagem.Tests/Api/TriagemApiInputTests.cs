using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Xunit;
using Xunit.Abstractions;

namespace TriagemImagem.Tests.Api;

[Collection("API")]
public sealed class TriagemApiInputTests : ApiTestBase
{
    public TriagemApiInputTests(ITestOutputHelper output) : base(output) { }

    [Fact]
    public async Task Calculo_de_dose_com_entrada_valida_retorna_200_e_volume()
    {
        await GarantirApiDisponivelAsync();
        var response = await Client.PostAsJsonAsync("api/v1/triagem/calcular-dose", TestDataFactory.CalculoDoseTcValido());
        var body = await LogResponseAsync("POST /api/v1/triagem/calcular-dose [80kg, TC]", response);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        using var json = JsonDocument.Parse(body);
        Assert.Equal(120m, json.RootElement.GetProperty("volumeAjustadoMl").GetDecimal());
    }

    [Fact]
    public async Task Calculo_de_dose_com_peso_invalido_retorna_400()
    {
        await GarantirApiDisponivelAsync();
        var response = await Client.PostAsJsonAsync("api/v1/triagem/calcular-dose", TestDataFactory.CalculoDosePesoInvalido());
        await LogResponseAsync("POST /api/v1/triagem/calcular-dose [peso inválido]", response);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Salvar_triagem_sem_respostas_retorna_400_sem_gravar_dados()
    {
        await GarantirApiDisponivelAsync();
        var response = await Client.PostAsJsonAsync("api/v1/triagem", TestDataFactory.TriagemSemRespostas());
        await LogResponseAsync("POST /api/v1/triagem [checklist vazio]", response);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Salvar_triagem_para_exame_inexistente_retorna_400_sem_gravar_dados()
    {
        await GarantirApiDisponivelAsync();
        var response = await Client.PostAsJsonAsync("api/v1/triagem", TestDataFactory.TriagemParaExameInexistente());
        await LogResponseAsync("POST /api/v1/triagem [exame inexistente]", response);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Liberar_e_cancelar_exame_inexistente_retornam_404()
    {
        await GarantirApiDisponivelAsync();
        var liberar = await Client.PostAsJsonAsync("api/v1/triagem/999999/liberar", new { usuarioId = 1, observacao = "Teste sem alteração" });
        await LogResponseAsync("POST /api/v1/triagem/999999/liberar", liberar);
        var cancelar = await Client.PostAsJsonAsync("api/v1/triagem/999999/cancelar", new { usuarioId = 1, motivo = "Teste sem alteração" });
        await LogResponseAsync("POST /api/v1/triagem/999999/cancelar", cancelar);
        Assert.Equal(HttpStatusCode.NotFound, liberar.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, cancelar.StatusCode);
    }
}
