namespace Atlas.Infrastructure.Options;

public sealed class TelegramBotOptions
{
    public const string SectionName = "Telegram";
    public const string TokenKey = $"{SectionName}:BotToken";
    public string BotToken { get; init; } = string.Empty;
}