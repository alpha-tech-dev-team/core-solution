using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Atlas.Core.Interfaces;
using Atlas.Core.Options;
using Atlas.Infrastructure.Access;
using Atlas.Infrastructure.Identity;
using Atlas.Infrastructure.Options;

namespace Atlas.Infrastructure.DependencyInjection;

public static class InfrastructureServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<AccessOptions>(configuration.GetSection(AccessOptions.SectionName));
        services.Configure<SessionOptions>(configuration.GetSection(SessionOptions.SectionName));
        services.AddSingleton<IAccessService, AccessService>();

        var identityOptions = configuration.GetSection(IdentityOptions.SectionName).Get<IdentityOptions>()
            ?? throw new InvalidOperationException($"Missing configuration section {IdentityOptions.SectionName}.");

        ArgumentException.ThrowIfNullOrWhiteSpace(identityOptions.BaseUrl);

        services.AddHttpClient<IIdentityClient, IdentityClient>(client =>
        {
            client.BaseAddress = new Uri(identityOptions.BaseUrl);
            client.Timeout = TimeSpan.FromSeconds(10);
        });

        return services;
    }
}
