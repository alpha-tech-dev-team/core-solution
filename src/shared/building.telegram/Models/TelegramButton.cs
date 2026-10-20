namespace Building.Telegram.Models;

public sealed class TelegramButton
{
    public TelegramButton(string text, string callbackData)
    {
        Text = text;
        CallbackData = callbackData;
    }

    public string Text { get; }

    public string CallbackData { get; }
}
