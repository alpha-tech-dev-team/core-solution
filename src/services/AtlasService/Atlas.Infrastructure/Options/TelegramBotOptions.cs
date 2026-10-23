namespace Atlas.Infrastructure.Options;

public sealed class TelegramBotOptions
{
    public const string SectionName = "Telegram";
    public string BotToken { get; init; } = string.Empty;
}
