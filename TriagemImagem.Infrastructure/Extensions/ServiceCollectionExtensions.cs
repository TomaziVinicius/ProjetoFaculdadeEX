using Microsoft.Extensions.DependencyInjection;
using TriagemImagem.Domain.Repositories;
using TriagemImagem.Domain.Services;
using TriagemImagem.Infrastructure.Repositories;

namespace TriagemImagem.Infrastructure.Extensions;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registra no container de Injeção de Dependência (.NET DI) os repositórios de infraestrutura
    /// e os serviços de domínio de triagem.
    /// </summary>
    public static IServiceCollection AddInfrastructureRepositories(this IServiceCollection services)
    {
        // Repositórios EF Core
        services.AddScoped<IExameRepository, ExameRepository>();
        services.AddScoped<IPacienteRepository, PacienteRepository>();
        services.AddScoped<ITriagemRepository, TriagemRepository>();
        services.AddScoped<IUsuarioRepository, UsuarioRepository>();
        services.AddScoped<IAuditoriaRepository, AuditoriaRepository>();
        services.AddScoped<IWorklistLabRepository, WorklistLabRepository>();

        // Serviços de Domínio Clínico
        services.AddScoped<ISemaforoClinicoDomainService, SemaforoClinicoDomainService>();

        return services;
    }
}
