using Microsoft.Extensions.Options;
using Pandora.Domain.Options;

namespace Pandora.Api.Extensions.Configuration;

public static class VaultwardenClientExtensions
{
    public static IServiceCollection AddVaultwardenClient<TClient, TService>(this IServiceCollection services)
        where TClient : class, TService
        where TService : class
    {
        services.AddHttpClient<TClient>((serviceProvider, client) =>
            client.BaseAddress = new Uri(serviceProvider.GetRequiredService<IOptions<VaultwardenOptions>>().Value.BaseUrl));

        services.AddScoped<TService>(provider => provider.GetRequiredService<TClient>());

        return services;
    }
}
