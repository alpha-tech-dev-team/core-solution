using Microsoft.Extensions.Options;
using Pandora.Domain.Interfaces;
using Pandora.Domain.Options;
using Pandora.Infrastructure.Secrets;

namespace Pandora.Api.Extensions.Configuration;

public static class SecretWriterExtensions
{
    public static IServiceCollection AddSecretWriter(this IServiceCollection services)
    {
        services.AddHttpClient<VaultwardenSecretWriter>((serviceProvider, client) =>
            client.BaseAddress = new Uri(serviceProvider.GetRequiredService<IOptions<VaultwardenOptions>>().Value.BaseUrl));

        services.AddScoped<ISecretWriter>(provider => provider.GetRequiredService<VaultwardenSecretWriter>());

        return services;
    }
}
