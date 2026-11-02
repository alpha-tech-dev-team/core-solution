using Atlas.Infrastructure.Repositories;
using Atlas.Infrastructure.Utils;
using Atlas.Core.Options;
using Atlas.Core.Interfaces;
using StackExchange.Redis;

namespace Atlas.TgBot.Extensions.DependencyInjection;

public static class SessionExtensions
{
    public static IServiceCollection AddSessionExtensions(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddHttpClient("IdentityClient", client =>
        {
            client.BaseAddress = new Uri(configuration.Get<IdentityOptions>()!.BaseUrl);
        });

        services.AddScoped<ISessionRepository, RedisSessionRepository>();
        services.AddScoped<IRequestContext, RequestContext>();

        return services;
    }
}
