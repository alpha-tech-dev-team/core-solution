namespace Atlas.TgBot.Options;

public sealed class TelegramOptions
{
    public const string SectionName = "Telegram";

    public string BotToken { get; set; } = string.Empty;
    public string NotificationBotToken { get; set; } = string.Empty;
}
