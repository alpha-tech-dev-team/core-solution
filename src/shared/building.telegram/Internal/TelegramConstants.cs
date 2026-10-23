namespace Building.Telegram.Internal;

internal static class TelegramConstants
{
    public const int MaxMessageLength = 4096;

    public static readonly TimeSpan PollingErrorDelay = TimeSpan.FromSeconds(5);
}
