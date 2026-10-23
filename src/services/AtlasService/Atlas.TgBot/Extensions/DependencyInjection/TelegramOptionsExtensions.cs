using Atlas.Infrastructure.Options;
using Building.Telegram.DependencyInjection;

namespace Atlas.TgBot.Extensions.DependencyInjection;

public static class TelegramOptionsExtensions
{
    public static IServiceCollection AddTelegramOptions(this IServiceCollection services, IConfiguration configuration)
    {
        var telegramOptions = configuration
            .GetSection(TelegramBotOptions.SectionName)
            .Get<TelegramBotOptions>()
            ?? throw new InvalidOperationException($"Section {TelegramBotOptions.SectionName} is not configured");

        ArgumentException.ThrowIfNullOrWhiteSpace(telegramOptions.BotToken);

        services.AddTelegramBot(telegramOptions.BotToken);
        return services;
    }
}