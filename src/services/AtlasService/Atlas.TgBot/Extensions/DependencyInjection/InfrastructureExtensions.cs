using Atlas.Domain.Options;
using Atlas.Infrastructure.Database.Extensions;
using CoreSolution.Shared.TgBot.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Atlas.TgBot.Extensions.DependencyInjection;

public static class InfrastructureExtensions
{
    public static IServiceCollection AddBotInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var databaseOptions = configuration.GetSection(DatabaseOptions.SectionName).Get<DatabaseOptions>()
            ?? throw new InvalidOperationException($"Missing configuration section {DatabaseOptions.SectionName}.");

        var redisOptions = configuration.GetSection(RedisOptions.SectionName).Get<RedisOptions>()
            ?? throw new InvalidOperationException($"Missing configuration section {RedisOptions.SectionName}.");

        services.AddAtlasDatabase(databaseOptions.ConnectionString);
        services.AddIdentityClient(configuration);
        services.AddSessionInfrastructure(configuration, redisOptions.ConnectionString);
        services.AddLocalization(typeof(Atlas.Infrastructure.AssemblyMarker).Assembly);

        return services;
    }
}
