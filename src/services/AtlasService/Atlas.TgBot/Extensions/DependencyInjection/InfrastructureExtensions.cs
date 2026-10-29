using Atlas.Core.Interfaces;
using Atlas.Infrastructure.Identity;
using Atlas.Infrastructure.Options;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Atlas.TgBot.Extensions.DependencyInjection;

public static class InfrastructureExtensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<Atlas.Core.Options.SessionOptions>(
            configuration.GetSection(Atlas.Core.Options.SessionOptions.SectionName));

        var identityOptions = configuration.GetSection(IdentityOptions.SectionName).Get<IdentityOptions>()
            ?? throw new InvalidOperationException($"Missing configuration section {IdentityOptions.SectionName}.");

        ArgumentException.ThrowIfNullOrWhiteSpace(identityOptions.BaseUrl);

        services.AddHttpClient<IIdentityClient, IdentityClient>(client =>
        {
            client.BaseAddress = new Uri(identityOptions.BaseUrl);
            client.Timeout = TimeSpan.Zero;
        });

        return services;
    }
}
