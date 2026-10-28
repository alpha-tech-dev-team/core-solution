using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Atlas.Core.Interfaces;
using Atlas.Infrastructure.Identity;
using Atlas.Infrastructure.Options;

namespace Atlas.Infrastructure.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddIdentityClient(this IServiceCollection  services, IConfiguration configuration)
    {
        var options = configuration.GetSection(IdentityOptions.SectionName).Get<IdentityOptions>()
            ?? throw new InvalidOperationException($"Missing config section: {IdentityOptions.SectionName}");
        if (string.IsNullOrWhiteSpace(options.BaseUrl))
        {
            throw new InvalidOperationException($"{IdentityOptions.SectionName}:BaseUrl is empty");
        }

        services.AddHttpClient<IIdentityClient, IdentityClient>(client =>
            {
            client.BaseAddress = new Uri(options.BaseUrl);
            client.Timeout = TimeSpan.FromSeconds(10);
            }
        );
        return services;
    }
}