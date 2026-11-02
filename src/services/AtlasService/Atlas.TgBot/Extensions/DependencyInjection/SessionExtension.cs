using Atlas.Infrastructure.Repositories;
using Atlas.Infrastructure.Utils;
using Atlas.Core.Interfaces;
using Atlas.Core.Options;
using Atlas.Infrastructure.Options;
using StackExchange.Redis;

namespace Atlas.TgBot.Extensions.DependencyInjection;

public static class SessionExtensions
{
    private const string HttpClient = "IdentityClient";
    public static IServiceCollection AddSessionExtensions(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton<IConnectionMultiplexer>(_ => 
            ConnectionMultiplexer.Connect(configuration.Get<DatabaseOptions>()!.RedisConnection));

        services.AddHttpClient(HttpClient, client =>
        {
            client.BaseAddress = new Uri(configuration.Get<IdentityOptions>()!.BaseUrl);
        });

        services.AddScoped<ISessionRepository, RedisSessionRepository>();
        services.AddScoped<IRequestContext, RequestContext>();
        
        return services;
    }
}
