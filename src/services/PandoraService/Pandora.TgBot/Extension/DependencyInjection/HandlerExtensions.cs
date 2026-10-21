using Building.Telegram.Abstractions;
using Pandora.TgBot.Handlers;

namespace Pandora.TgBot.Extension.DependencyInjection;

public static class HandlerExtensions
{
    public static IServiceCollection AddBotHandlers(this IServiceCollection services)
    {
        services.AddScoped<IBotHandler, StartCommandHandler>();
        services.AddScoped<IBotHandler, ServiceCallbackHandler>();

        return services;
    }
}
