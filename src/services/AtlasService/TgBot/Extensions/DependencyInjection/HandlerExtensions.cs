using Building.Telegram.Abstractions;

namespace Atlas.TgBot.Extensions.DependencyInjection;

public static class HandlerExtensions
{
    public static IServiceCollection AddBotHandlers(this IServiceCollection services)
    {
        var handlerTypes = typeof(AssemblyMarker).Assembly
            .GetTypes()
            .Where(type => type is { IsClass: true, IsAbstract: false }
                && typeof(IBotHandler).IsAssignableFrom(type));

        foreach (var handlerType in handlerTypes)
        {
            services.AddScoped(typeof(IBotHandler), handlerType);
        }

        return services;
    }
}