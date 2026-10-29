using Building.Telegram.Abstractions;

namespace Atlas.TgBot.Extensions.DependencyInjection;

public static class HandlerExtensions
{
    public static IServiceCollection AddBotHandlers(this IServiceCollection services, Func<Type, int>? prioritySelector = null)
    {
        var handlerTypes = typeof(AssemblyMarker).Assembly.GetTypes()
            .Where(type => type is { IsClass: true, IsAbstract: false } && typeof(IBotHandler).IsAssignableFrom(type));

        if (prioritySelector != null)
        {
            handlerTypes = handlerTypes.OrderBy(prioritySelector);
        }

        foreach (var handlerType in handlerTypes)
        {
            services.AddScoped(handlerType);
            services.AddScoped(typeof(IBotHandler), provider => provider.GetRequiredService(handlerType));
        }

        return services;
    }
}
