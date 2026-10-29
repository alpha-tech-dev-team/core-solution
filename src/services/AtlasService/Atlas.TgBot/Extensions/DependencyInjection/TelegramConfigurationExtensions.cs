using Atlas.Infrastructure.Options;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Atlas.TgBot.Extensions.DependencyInjection;

public static class TelegramConfigurationExtensions
{
    public static TelegramBotOptions AddAndValidateTelegramOptions(this IServiceCollection services, IConfiguration configuration)
    {
        var telegramOptions = configuration
            .GetSection(TelegramBotOptions.SectionName)
            .Get<TelegramBotOptions>()
            ?? throw new InvalidOperationException($"Section {TelegramBotOptions.SectionName} is missing.");

        if (string.IsNullOrWhiteSpace(telegramOptions.BotToken))
        {
            throw new InvalidOperationException($"{TelegramBotOptions.SectionName}:{nameof(TelegramBotOptions.BotToken)} is missing.");
        }

        services.Configure<TelegramBotOptions>(configuration.GetSection(TelegramBotOptions.SectionName));

        return telegramOptions;
    }
}
