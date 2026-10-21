using Building.Telegram.Models;
using Telegram.Bot.Types.ReplyMarkups;

namespace Building.Telegram.Extensions;

public static class TelegramMarkupExtensions
{
    public static InlineKeyboardMarkup? ToInlineKeyboardMarkup(this IEnumerable<TelegramButtonRow>? rows)
    {
        if (rows is null)
        {
            return null;
        }

        var keyboard = new List<IEnumerable<InlineKeyboardButton>>();

        foreach (var row in rows)
        {
            var buttons = new List<InlineKeyboardButton>();

            foreach (var button in row.Buttons)
            {
                buttons.Add(InlineKeyboardButton.WithCallbackData(button.Text, button.CallbackData));
            }

            if (buttons.Count > 0)
            {
                keyboard.Add(buttons);
            }
        }

        if (keyboard.Count == 0)
        {
            return null;
        }

        return new InlineKeyboardMarkup(keyboard);
    }

    public static InlineKeyboardMarkup? ToInlineKeyboardMarkup(this TelegramMessageContent content)
    {
        ArgumentNullException.ThrowIfNull(content);

        return content.ButtonRows.ToInlineKeyboardMarkup();
    }
}
