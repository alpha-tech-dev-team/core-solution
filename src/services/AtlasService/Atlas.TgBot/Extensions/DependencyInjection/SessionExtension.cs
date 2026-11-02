using Atlas.Infrastructure.Repositories;
using Atlas.Infrastructure.Utils;
using Atlas.Core.Interfaces;
using Atlas.Core.Options;
using Atlas.Infrastructure.Options;
using StackExchange.Redis;
using Atlas.Infrastructure.Identity;

namespace Atlas.TgBot.Extensions.DependencyInjection;

public static class SessionExtensions
{
    public static IServiceCollection AddSessionExtensions(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton<IConnectionMultiplexer>(_ => 
            ConnectionMultiplexer.Connect(configuration.Get<DatabaseOptions>()!.RedisConnection));

        services.AddHttpClient<IIdentityClient, IdentityClient>(client =>
        {
            client.BaseAddress = new Uri(configuration.Get<IdentityOptions>()!.BaseUrl);
        });

        services.AddScoped<ISessionRepository, RedisSessionRepository>();
        services.AddScoped<IRequestContext, RequestContext>();

        return services;
    }
}
