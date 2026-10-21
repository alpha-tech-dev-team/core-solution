using Building.Telegram.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Pandora.TgBot.Handlers;

namespace Pandora.TgBot.Extension.DI;

public static class HandlerExtensions
{
    public static IServiceCollection AddBotHandlers(this IServiceCollection services)
    {
        services.AddScoped<IBotHandler, StartCommandHandler>();

        return services;
    }
}
