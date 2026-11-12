using Atlas.Domain.Interfaces.Repositories;
using Atlas.Infrastructure.Database.Context;
using Atlas.Infrastructure.Database.Extensions;
using Atlas.Infrastructure.Repositories;
using Atlas.Domain.Options;
using CoreSolution.Shared.TgBot.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Pgvector.EntityFrameworkCore;

namespace Atlas.TgBot.Extensions.DependencyInjection;

public static class InfrastructureExtensions
{
    public static IServiceCollection AddBotInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var databaseOptions = configuration.GetSection(DatabaseOptions.SectionName).Get<DatabaseOptions>()
            ?? throw new InvalidOperationException($"Missing configuration section {DatabaseOptions.SectionName}.");

        var redisOptions = configuration.GetSection(RedisOptions.SectionName).Get<RedisOptions>()
            ?? throw new InvalidOperationException($"Missing configuration section {RedisOptions.SectionName}.");

        ArgumentException.ThrowIfNullOrWhiteSpace(databaseOptions.ConnectionString);
        ArgumentException.ThrowIfNullOrWhiteSpace(redisOptions.ConnectionString);

        services.Configure<DatabaseOptions>(configuration.GetSection(DatabaseOptions.SectionName));

        services.AddDbContext<AtlasDbContext>(options =>
            options.UseNpgsql(databaseOptions.ConnectionString, npgsql =>
            {
                npgsql.UseVector();
                npgsql.MapAtlasEnums();
            }));

        services.AddScoped<IIncidentCategoryRepository, IncidentCategoryRepository>();

        services.AddIdentityClient(configuration);
        services.AddSessionInfrastructure(configuration, redisOptions.ConnectionString);
        services.AddLocalization(typeof(Atlas.Infrastructure.AssemblyMarker).Assembly);

        return services;
    }
}
