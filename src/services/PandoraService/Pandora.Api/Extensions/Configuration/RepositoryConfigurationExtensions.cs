using Pandora.Domain.Interfaces;
using Pandora.Infrastructure.Repositories;

namespace Pandora.Api.Extensions.Configuration;

public static class RepositoryConfigurationExtensions
{
    public static IServiceCollection AddRepositoriesConfiguration(this IServiceCollection services)
    {
        services.AddScoped<ISecretMetadataRepository, SecretMetadataRepository>();
        services.AddScoped<IPermissionRepository, PermissionRepository>();

        return services;
    }
}
