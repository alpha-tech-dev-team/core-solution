using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Pandora.Domain.Interfaces;
using Pandora.Infrastructure.Secrets;

namespace Pandora.TgBot.Extension.DependencyInjection;

public static class SecretStoreExtensions
{
    public static IServiceCollection AddSecretStore(this IServiceCollection services, IConfiguration configuration)
    {
        var options = configuration.GetSection(SecretsOptions.SectionName).Get<SecretsOptions>() ?? new SecretsOptions();

        if (options.UseMock)
        {
            services.AddScoped<ConfigSecretStore>();
            services.AddScoped<ISecretStore>(provider => provider.GetRequiredService<ConfigSecretStore>());
            services.AddScoped<ISecretCatalog>(provider => provider.GetRequiredService<ConfigSecretStore>());

            return services;
        }

        var vaultwardenOptions = configuration.GetSection(VaultwardenOptions.SectionName).Get<VaultwardenOptions>() ?? new VaultwardenOptions();

        ArgumentException.ThrowIfNullOrWhiteSpace(vaultwardenOptions.BaseUrl);

        services.AddHttpClient<VaultwardenSecretStore>(client => client.BaseAddress = new Uri(vaultwardenOptions.BaseUrl));
        services.AddScoped<ISecretStore>(provider => provider.GetRequiredService<VaultwardenSecretStore>());
        services.AddScoped<ISecretCatalog>(provider => provider.GetRequiredService<VaultwardenSecretStore>());

        return services;
    }
}
