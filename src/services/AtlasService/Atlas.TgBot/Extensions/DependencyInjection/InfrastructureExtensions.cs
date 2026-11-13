using Atlas.Domain.Interfaces.Repositories;
using Atlas.Domain.Options;
using Atlas.Infrastructure.Database.Context;
using Atlas.Infrastructure.Database.Extensions;
using Atlas.Infrastructure.Repositories;
using Atlas.TgBot.BotHandlers;
using Atlas.TgBot.Options;
using CoreSolution.Shared.TgBot.Abstractions;
using CoreSolution.Shared.TgBot.BotHandlers;
using CoreSolution.Shared.TgBot.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Pgvector.EntityFrameworkCore;

namespace Atlas.TgBot.Extensions.DependencyInjection;

public static class InfrastructureExtensions
{
    public static IServiceCollection AddBotInfrastructure(this IServiceCollection services, IConfiguration configuration, IHostEnvironment environment)
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

        services.Configure<PhoneConfirmationOptions>(configuration.GetSection(PhoneConfirmationOptions.SectionName));

        RegisterStartHandler(services, configuration, environment);

        return services;
    }

    private static void RegisterStartHandler(IServiceCollection services, IConfiguration configuration, IHostEnvironment environment)
    {
        var options = configuration.GetSection(PhoneConfirmationOptions.SectionName).Get<PhoneConfirmationOptions>()
            ?? new PhoneConfirmationOptions();

        if (options.Enabled || environment.IsProduction())
        {
            return;
        }

        services.Remove(ServiceDescriptor.Scoped<IBotHandler, ContactBotHandler>());
    }
}
