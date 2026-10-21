using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Pandora.Core.Interfaces;
using Pandora.Infrastructure.Access;
using Pandora.Infrastructure.Secrets;

namespace Pandora.Infrastructure.DependencyInjection;

public static class InfrastructureServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<AccessOptions>(configuration.GetSection("Pandora:" + AccessOptions.SectionName));
        services.AddScoped<IAccessService, ConfigAccessService>();

        services.Configure<SecretsOptions>(configuration.GetSection("Pandora:" + SecretsOptions.SectionName));
        services.AddScoped<ISecretStore, ConfigSecretStore>();

        return services;
    }
}
