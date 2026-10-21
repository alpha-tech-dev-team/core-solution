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
        services.AddPandoraOptions(configuration);
        services.AddScoped<IAccessService, ConfigAccessService>();
        services.AddScoped<ISecretStore, ConfigSecretStore>();

        return services;
    }
}
