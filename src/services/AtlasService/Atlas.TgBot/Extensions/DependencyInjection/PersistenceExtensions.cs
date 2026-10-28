using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Atlas.Core.Interfaces;
using Atlas.Core.Options;
using Atlas.Infrastructure.Repositories;
using Atlas.Infrastructure.Utils;
using StackExchange.Redis;

namespace Atlas.TgBot.Extensions.DependencyInjection;

public static class PersistenceExtensions
{
    public static IServiceCollection AddPersistence(this IServiceCollection services, IConfiguration configuration)
    {
        var databaseOptions = configuration.GetSection(DatabaseOptions.SectionName).Get<DatabaseOptions>()
            ?? throw new InvalidOperationException($"Missing configuration section {DatabaseOptions.SectionName}.");

        services.AddSingleton<IConnectionMultiplexer>(_ => ConnectionMultiplexer.Connect(databaseOptions.RedisConnection));
        services.AddScoped<ISessionRepository, RedisSessionRepository>();
        services.AddScoped<IRequestContext, RequestContext>();

        return services;
    }
}
