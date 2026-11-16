using Microsoft.Extensions.Options;
using Pandora.Domain.Interfaces;
using Pandora.Domain.Options;
using Pandora.Infrastructure.Secrets;

namespace Pandora.Api.Extensions.Configuration;

public static class SecretVaultReaderExtensions
{
    public static IServiceCollection AddSecretVaultReader(this IServiceCollection services)
    {
        services.AddHttpClient<VaultwardenSecretStore>((serviceProvider, client) =>
            client.BaseAddress = new Uri(serviceProvider.GetRequiredService<IOptions<VaultwardenOptions>>().Value.BaseUrl));

        services.AddScoped<ISecretVaultReader>(provider => provider.GetRequiredService<VaultwardenSecretStore>());

        return services;
    }
}
