using Telegram.Bot.Types;

namespace CoreSolution.Shared.TgBot.Internal.UpdatePipeline;

internal static class UpdateContext
{
    public static (long? TgUserId, int? MessageId) Extract(Update update)
    {
        if (update.Message is { } message)
        {
            return (message.Chat.Id, message.MessageId);
        }

        if (update.CallbackQuery is { Message: { } callbackMessage })
        {
            return (callbackMessage.Chat.Id, callbackMessage.MessageId);
        }

        return (null, null);
    }
}
