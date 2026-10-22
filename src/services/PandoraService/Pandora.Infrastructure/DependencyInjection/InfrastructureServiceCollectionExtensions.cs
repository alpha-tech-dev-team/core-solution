using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Pandora.Core.Interfaces;
using Pandora.Infrastructure.Access;
using Pandora.Infrastructure.Identity;
using Pandora.Infrastructure.Secrets;

namespace Pandora.Infrastructure.DependencyInjection;

public static class InfrastructureServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var identityOptions = configuration.GetSection(IdentityOptions.SectionName).Get<IdentityOptions>()
            ?? throw new InvalidOperationException($"Missing configuration section {IdentityOptions.SectionName}.");

        if (string.IsNullOrWhiteSpace(identityOptions.BaseUrl))
        {
            throw new InvalidOperationException($"Missing identity base url in section {IdentityOptions.SectionName}.");
        }

        services.AddHttpClient<IIdentityClient, IdentityClient>(client => client.BaseAddress = new Uri(identityOptions.BaseUrl));
        services.AddScoped<IAccessService, IdentityAccessService>();
        services.AddScoped<ISecretStore, ConfigSecretStore>();

        return services;
    }
}
