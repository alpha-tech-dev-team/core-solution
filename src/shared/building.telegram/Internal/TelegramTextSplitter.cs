namespace Building.Telegram.Internal;

internal static class TelegramTextSplitter
{
    public static IReadOnlyList<string> Split(string text)
    {
        if (string.IsNullOrEmpty(text))
        {
            return [string.Empty];
        }

        if (text.Length <= TelegramConstants.MaxMessageLength)
        {
            return [text];
        }

        var parts = new List<string>();
        var index = 0;

        while (index < text.Length)
        {
            var length = Math.Min(TelegramConstants.MaxMessageLength, text.Length - index);

            if (index + length < text.Length)
            {
                var breakAt = text.LastIndexOf('\n', index + length - 1, length);

                if (breakAt > index)
                {
                    length = breakAt - index + 1;
                }
            }

            parts.Add(text.Substring(index, length));
            index += length;
        }

        return parts;
    }
}
