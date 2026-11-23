using Atlas.Domain.Interfaces.Services;
using Atlas.Domain.Interfaces.Repositories;
using Atlas.Domain.Options;
using Atlas.Infrastructure.Database.Context;
using Atlas.Infrastructure.Database.Extensions;
using Atlas.Infrastructure.Notifications;
using Atlas.Infrastructure.Repositories;
using Atlas.Infrastructure.Sessions;
using Atlas.TgBot.Options;
using CoreSolution.Shared.Abstractions;
using CoreSolution.Shared.TgBot.Abstractions;
using CoreSolution.Shared.TgBot.DependencyInjection;
using CoreSolution.Shared.TgBot.Internal.Notifications;
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
    public const string NotificationServiceKey = "notification";

    public static IServiceCollection AddBotInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var databaseOptions = configuration.GetSection(DatabaseOptions.SectionName).Get<DatabaseOptions>()
            ?? throw new InvalidOperationException($"Missing configuration section {DatabaseOptions.SectionName}.");

        var redisOptions = configuration.GetSection(RedisOptions.SectionName).Get<RedisOptions>()
            ?? throw new InvalidOperationException($"Missing configuration section {RedisOptions.SectionName}.");

        var telegramOptions = configuration.GetSection(TelegramOptions.SectionName).Get<TelegramOptions>()
            ?? throw new InvalidOperationException($"Missing configuration section {TelegramOptions.SectionName}.");

        ArgumentException.ThrowIfNullOrWhiteSpace(databaseOptions.ConnectionString);
        ArgumentException.ThrowIfNullOrWhiteSpace(redisOptions.ConnectionString);
        ArgumentException.ThrowIfNullOrWhiteSpace(telegramOptions.NotificationBotToken);

        services.Configure<DatabaseOptions>(configuration.GetSection(DatabaseOptions.SectionName));

        services.AddDbContext<AtlasDbContext>(options =>
            options.UseNpgsql(databaseOptions.ConnectionString, npgsql =>
            {
                npgsql.UseVector();
                npgsql.MapAtlasEnums();
            }));

        services.AddScoped<IIncidentCategoryRepository, IncidentCategoryRepository>();
        services.AddScoped<IRoomRepository, RoomRepository>();

        services.AddIdentityClient(configuration);
        services.Replace(ServiceDescriptor.Scoped<ISessionFactory, AtlasSessionFactory>());
        services.AddSessionInfrastructure(configuration, redisOptions.ConnectionString);
        services.AddLocalization(typeof(Atlas.Infrastructure.AssemblyMarker).Assembly);

        services.Configure<TelegramOptions>(configuration.GetSection(TelegramOptions.SectionName));

        services.AddKeyedSingleton<ITelegramBotClient>(
            NotificationServiceKey,
            (_, _) => new TelegramBotClient(telegramOptions.NotificationBotToken));
        services.AddKeyedSingleton<ITelegramNotificationService>(
            NotificationServiceKey,
            (provider, _) => new TelegramNotificationService(
                provider.GetRequiredKeyedService<ITelegramBotClient>(NotificationServiceKey),
                provider.GetRequiredService<ILogger<TelegramNotificationService>>()));

        services.AddSingleton<IIncidentNotifier>(provider => new IncidentNotifier(
            provider.GetRequiredKeyedService<ITelegramNotificationService>(NotificationServiceKey)));

        return services;
    }
}
