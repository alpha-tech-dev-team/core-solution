using Pandora.Domain.Interfaces;
using Pandora.Infrastructure.Secrets;

namespace Pandora.Api.Extensions.Configuration;

public static class SecretsConfigurationExtensions
{
    public static IServiceCollection AddSecretsConfiguration(this IServiceCollection services)
    {
        services.AddVaultwardenClient<VaultwardenSecretWriter, ISecretWriter>();
        services.AddVaultwardenClient<VaultwardenSecretStore, ISecretVaultReader>();

        return services;
    }
}
