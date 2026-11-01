using Atlas.Infrastructure.Repositories;
using Atlas.Infrastructure.Utils;
using Atlas.Core.Options;
using Atlas.Core.Interfaces;
using StackExchange.Redis;

namespace Atlas.TgBot.Extensions.DependencyInjection;

public static class SessionExtensions
{
    public static IServiceCollection AddSessionOptions(this IServiceCollection services, IConfiguration configuration)
    {
        var databaseSection = configuration.GetSection(DatabaseOptions.SectionName);
        services.Configure<DatabaseOptions>(databaseSection);
        var databaseOptions = databaseSection.Get<DatabaseOptions>()
            ?? throw new InvalidOperationException($"Missing configuration section {DatabaseOptions.SectionName}.");

        services.AddSingleton<IConnectionMultiplexer>(_ => 
            ConnectionMultiplexer.Connect(databaseOptions.RedisConnection));

        services.AddScoped<ISessionRepository, RedisSessionRepository>();
        services.AddScoped<IRequestContext, RequestContext>();

        return services;
    }
}
