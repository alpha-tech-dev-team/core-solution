using Atlas.Infrastructure.Options;

namespace Atlas.TgBot.Extensions.DependencyInjection;

public static class TelegramOptionsExtensions
{
    public static TelegramBotOptions GetTelegramOptions(this IConfiguration configuration)
    {
        var telegramOptions = configuration
        .GetSection(TelegramBotOptions.SectionName)
        .Get<TelegramBotOptions>()
        ?? throw new InvalidOperationException($"Секция {TelegramBotOptions.SectionName} не задана");

        if (string.IsNullOrWhiteSpace(telegramOptions.BotToken))
        {
            throw new InvalidOperationException(
                $"{TelegramBotOptions.SectionName}:{nameof(TelegramBotOptions.BotToken)} не задан");
        }

        return telegramOptions;

    }
}