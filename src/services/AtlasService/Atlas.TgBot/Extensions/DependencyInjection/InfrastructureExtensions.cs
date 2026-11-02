using Atlas.Core.Options;
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

        services.AddIdentityClient(configuration);
        services.AddSessionInfrastructure(configuration, databaseOptions.RedisConnection);
        services.AddLocalization(typeof(Atlas.Infrastructure.AssemblyMarker).Assembly);

        return services;
    }
}
