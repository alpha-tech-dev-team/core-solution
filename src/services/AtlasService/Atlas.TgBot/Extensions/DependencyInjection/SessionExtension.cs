using Atlas.Infrastructure.Repositories;
using Atlas.Infrastructure.Utils;
using Atlas.Core.Interfaces;
using Atlas.Core.Options;
using Atlas.Infrastructure.Identity;
using StackExchange.Redis;

namespace Atlas.TgBot.Extensions.DependencyInjection;

public static class SessionExtensions
{
    public static IServiceCollection AddSessionExtensions(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton<IConnectionMultiplexer>(_ => 
            ConnectionMultiplexer.Connect(configuration.Get<DatabaseOptions>()!.RedisConnection));

        services.AddHttpClient<IIdentityClient, IdentityClient>();
        services.AddScoped<ISessionRepository, RedisSessionRepository>();
        services.AddScoped<IRequestContext, RequestContext>();

        return services;
    }
}
