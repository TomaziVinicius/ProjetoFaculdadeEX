using Microsoft.Extensions.DependencyInjection;
using TriagemImagem.Application.Interfaces;
using TriagemImagem.Application.Services;

namespace TriagemImagem.Application.Extensions;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registra no container de Injeção de Dependência (.NET DI) os serviços da camada de aplicação.
    /// </summary>
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<IAuthAppService, AuthAppService>();
        services.AddScoped<IWorklistAppService, WorklistAppService>();
        services.AddScoped<ITriagemAppService, TriagemAppService>();
        services.AddScoped<ICalculoDoseAppService, CalculoDoseAppService>();
        services.AddScoped<IAuditoriaAppService, AuditoriaAppService>();

        return services;
    }
}
