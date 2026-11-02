using Atlas.Infrastructure.Repositories;
using Atlas.Infrastructure.Utils;
using Atlas.Core.Interfaces;
using Atlas.Infrastructure.Options;

namespace Atlas.TgBot.Extensions.DependencyInjection;

public static class SessionExtensions
{
    public static IServiceCollection AddSessionOptions(this IServiceCollection services, IConfiguration configuration)
    {
        var identitySection = configuration.GetSection(IdentityOptions.SectionName);
        var identityOptions = identitySection.Get<IdentityOptions>() 
            ?? throw new InvalidOperationException($"Missing configuration section {IdentityOptions.SectionName}.");
        
        ArgumentException.ThrowIfNullOrWhiteSpace(identityOptions.BaseUrl);

        services.AddHttpClient("IdentityClient", client =>
        {
            client.BaseAddress = new Uri(identityOptions.BaseUrl);
        });

        services.AddScoped<ISessionRepository, RedisSessionRepository>();
        services.AddScoped<IRequestContext, RequestContext>();

        return services;
    }
}
