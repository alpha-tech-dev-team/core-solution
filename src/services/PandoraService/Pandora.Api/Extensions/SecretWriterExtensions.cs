using Microsoft.Extensions.Configuration;
using Pandora.Domain.Interfaces;
using Pandora.Domain.Options;
using Pandora.Infrastructure.Secrets;

namespace Pandora.Api.Extensions;

public static class SecretWriterExtensions
{
    public static IServiceCollection AddSecretWriter(this IServiceCollection services, IConfiguration configuration)
    {
        var options = configuration.GetSection(VaultwardenOptions.SectionName).Get<VaultwardenOptions>() ?? new VaultwardenOptions();

        ArgumentException.ThrowIfNullOrWhiteSpace(options.BaseUrl);

        services.AddHttpClient<VaultwardenSecretWriter>(client => client.BaseAddress = new Uri(options.BaseUrl));
        services.AddScoped<ISecretWriter>(provider => provider.GetRequiredService<VaultwardenSecretWriter>());

        return services;
    }
}
