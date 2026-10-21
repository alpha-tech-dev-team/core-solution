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

    public static ReplyMarkup? ToReplyMarkup(this TelegramMessageContent? content, string contactButtonText)
    {
        if (content is null)
        {
            return null;
        }

        if (content.RequestContact)
        {
            return new ReplyKeyboardMarkup(new KeyboardButton(contactButtonText) { RequestContact = true })
            {
                ResizeKeyboard = true,
                OneTimeKeyboard = true
            };
        }

        return content.ButtonRows.ToInlineKeyboardMarkup();
    }
}
