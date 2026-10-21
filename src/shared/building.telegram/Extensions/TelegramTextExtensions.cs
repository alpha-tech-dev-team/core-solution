using Building.Telegram.Internal;

namespace Building.Telegram.Extensions;

public static class TelegramTextExtensions
{
    private const string Ellipsis = "…";

    public static string ClampForTelegram(this string text)
    {
        if (string.IsNullOrEmpty(text) || text.Length <= TelegramConstants.MaxMessageLength)
        {
            return text;
        }

        var budget = TelegramConstants.MaxMessageLength - Ellipsis.Length;

        if (char.IsHighSurrogate(text[budget - 1]))
        {
            budget--;
        }

        return string.Concat(text.AsSpan(0, budget), Ellipsis);
    }
}
