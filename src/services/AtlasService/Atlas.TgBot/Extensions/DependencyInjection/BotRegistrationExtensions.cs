using Atlas.TgBot;
using CoreSolution.Shared.TgBot.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Atlas.TgBot.Extensions.DependencyInjection;

public static class BotRegistrationExtensions
{
    public static IServiceCollection AddAtlasBot(this IServiceCollection services, IConfiguration configuration)
    {
        // ContactBotHandler регистрируется внутри AddTelegramBot, поэтому порядок вызовов задаёт приоритет обработчиков.
        services.AddTelegramBot(configuration);
        services.AddBotHandlers<AssemblyMarker>();

        return services;
    }
}
