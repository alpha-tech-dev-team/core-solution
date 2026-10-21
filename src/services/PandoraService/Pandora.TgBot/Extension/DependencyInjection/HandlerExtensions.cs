using Building.Telegram.Abstractions;
using Pandora.TgBot.BotHandlers;

namespace Pandora.TgBot.Extension.DependencyInjection;

public static class HandlerExtensions
{
    public static IServiceCollection AddBotHandlers(this IServiceCollection services)
    {
        services.AddScoped<IBotHandler, StartBotHandler>();
        services.AddScoped<IBotHandler, ServiceCallbackBotHandler>();

        return services;
    }
}
