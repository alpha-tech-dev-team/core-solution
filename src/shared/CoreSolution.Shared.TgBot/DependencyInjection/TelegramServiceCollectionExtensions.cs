using CoreSolution.Shared.TgBot.Abstractions;
using CoreSolution.Shared.TgBot.BotHandlers;
using CoreSolution.Shared.TgBot.Internal.Hosting;
using CoreSolution.Shared.TgBot.Internal.Notifications;
using CoreSolution.Shared.TgBot.Internal.UpdatePipeline;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Telegram.Bot;

namespace CoreSolution.Shared.TgBot.DependencyInjection;

public static class TelegramServiceCollectionExtensions
{
    private const string BotTokenKey = "Telegram:BotToken";

    public static IServiceCollection AddTelegramBot(this IServiceCollection services, IConfiguration configuration)
    {
        var token = configuration[BotTokenKey] ?? string.Empty;

        return services.AddTelegramBot(token);
    }

    public static IServiceCollection AddTelegramBot(this IServiceCollection services, string token)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(token);

        services.AddSingleton<ITelegramBotClient>(_ => new TelegramBotClient(token));
        services.AddSingleton<ITelegramNotificationService, TelegramNotificationService>();
        services.AddScoped<IBotUpdateHandler, BotUpdateHandler>();
        services.AddScoped<IBotHandler, ContactBotHandler>();
        services.AddHostedService<BotWorker>();

        return services;
    }

    public static IServiceCollection AddBotHandlers<TMarker>(this IServiceCollection services)
    {
        var handlerTypes = typeof(TMarker).Assembly.GetTypes()
            .Where(type => type is { IsClass: true, IsAbstract: false } && typeof(IBotHandler).IsAssignableFrom(type));

        foreach (var handlerType in handlerTypes)
        {
            services.AddScoped(typeof(IBotHandler), handlerType);
        }

        return services;
    }
}
