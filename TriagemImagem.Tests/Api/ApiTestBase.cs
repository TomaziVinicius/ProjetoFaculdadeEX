using System.Net.Http.Json;
using Xunit;
using Xunit.Abstractions;

namespace TriagemImagem.Tests.Api;

[CollectionDefinition("API", DisableParallelization = true)]
public sealed class ApiCollection { }

public abstract class ApiTestBase
{
    protected ApiTestBase(ITestOutputHelper output)
    {
        Output = output;
        var baseUrl = Environment.GetEnvironmentVariable("API_BASE_URL") ?? "http://localhost:5000";
        Client = new HttpClient { BaseAddress = new Uri(baseUrl.TrimEnd('/') + "/") };
    }

    protected HttpClient Client { get; }
    protected ITestOutputHelper Output { get; }

    protected async Task<string> LogResponseAsync(string request, HttpResponseMessage response)
    {
        var body = await response.Content.ReadAsStringAsync();
        Output.WriteLine($"{request} => {(int)response.StatusCode} ({response.StatusCode})");
        Output.WriteLine(body);
        return body;
    }
}
