using CoreSolution.Shared.Infrastructure.Identity;
using CoreSolution.Shared.Abstractions;
using CoreSolution.Shared.Options;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CoreSolution.Shared.TgBot.DependencyInjection;

public static class IdentityServiceCollectionExtensions
{
    private static readonly TimeSpan RequestTimeout = TimeSpan.FromSeconds(10);

    public static IServiceCollection AddIdentityClient(this IServiceCollection services, IConfiguration configuration)
    {
        var section = configuration.GetSection(IdentityOptions.SectionName);
        var identityOptions = section.Get<IdentityOptions>()
            ?? throw new InvalidOperationException($"Missing configuration section {IdentityOptions.SectionName}.");

        ArgumentException.ThrowIfNullOrWhiteSpace(identityOptions.BaseUrl, $"{IdentityOptions.SectionName}:{nameof(IdentityOptions.BaseUrl)}");

        services.Configure<IdentityOptions>(section);
        services.AddHttpClient<IIdentityClient, IdentityClient>(client =>
        {
            client.BaseAddress = new Uri(identityOptions.BaseUrl);
            client.Timeout = RequestTimeout;
        });

        return services;
    }
}
