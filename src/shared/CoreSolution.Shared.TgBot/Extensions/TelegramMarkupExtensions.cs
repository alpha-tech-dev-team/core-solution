using CoreSolution.Shared.Models;
using Telegram.Bot.Types.ReplyMarkups;

namespace CoreSolution.Shared.TgBot.Extensions;

public static class TelegramMarkupExtensions
{
    public static InlineKeyboardMarkup? ToInlineKeyboardMarkup(this IEnumerable<WorkflowButtonRow>? rows)
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
}
