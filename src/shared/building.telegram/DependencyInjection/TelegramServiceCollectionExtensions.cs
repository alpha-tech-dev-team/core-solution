using Building.Telegram.Abstractions;
using Building.Telegram.Internal;
using Microsoft.Extensions.DependencyInjection;
using Telegram.Bot;

namespace Building.Telegram.DependencyInjection;

public static class TelegramServiceCollectionExtensions
{
    public static IServiceCollection AddTelegramBot(this IServiceCollection services, string token)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(token);

        services.AddSingleton<ITelegramBotClient>(_ => new TelegramBotClient(token));
        services.AddSingleton<ITelegramNotificationService, TelegramNotificationService>();
        services.AddScoped<IBotUpdateHandler, BotUpdateHandler>();
        services.AddHostedService<BotWorker>();

        return services;
    }
}
