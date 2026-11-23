using Atlas.Application.Abstractions;
using Atlas.Domain.Interfaces.Repositories;
using Atlas.Domain.Options;
using Atlas.Infrastructure.Database.Context;
using Atlas.Infrastructure.Database.Extensions;
using Atlas.Infrastructure.Repositories;
using Atlas.Infrastructure.Sessions;
using Atlas.TgBot.Notifications;
using CoreSolution.Shared.Abstractions;
using CoreSolution.Shared.TgBot.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Pgvector.EntityFrameworkCore;
using Telegram.Bot;

namespace Atlas.TgBot.Extensions.DependencyInjection;

public static class InfrastructureExtensions
{
    private const string NotificationBotClientServiceKey = "notification";
    private const string NotificationBotTokenKey = "Telegram:NotificationBotToken";

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
        services.Replace(ServiceDescriptor.Scoped<ISessionFactory, AtlasSessionFactory>());
        services.AddSessionInfrastructure(configuration, redisOptions.ConnectionString);
        services.AddLocalization(typeof(Atlas.Infrastructure.AssemblyMarker).Assembly);

        AddNotificationBot(services, configuration);

        return services;
    }

    private static void AddNotificationBot(IServiceCollection services, IConfiguration configuration)
    {
        var notificationBotToken = configuration[NotificationBotTokenKey];
        ArgumentException.ThrowIfNullOrWhiteSpace(notificationBotToken);
        services.AddKeyedSingleton<ITelegramBotClient>(
            NotificationBotClientServiceKey,
            (_, _) => new TelegramBotClient(notificationBotToken));
        services.AddSingleton<ITelegramMessageSender>(provider => new TelegramMessageSender(
            provider.GetRequiredKeyedService<ITelegramBotClient>(NotificationBotClientServiceKey),
            provider.GetRequiredService<ILogger<TelegramMessageSender>>()));
    }
}
