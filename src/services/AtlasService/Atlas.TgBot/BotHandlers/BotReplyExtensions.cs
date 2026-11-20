using Atlas.Domain.Models;
using CoreSolution.Shared.Abstractions;
using CoreSolution.Shared.TgBot.Abstractions;
using CoreSolution.Shared.TgBot.Extensions;
using Telegram.Bot.Types.ReplyMarkups;

namespace Atlas.TgBot.BotHandlers;

/// <summary>
/// Единая отправка ответа бота: удаляет предыдущее сообщение бота и запоминает id нового,
/// чтобы следующий ответ снова удалял именно сообщение бота, а не сообщение пользователя.
/// </summary>
public static class BotReplyExtensions
{
    public static async Task SendReplyAsync(
        this ITelegramNotificationService telegram,
        IRequestContext requestContext,
        ISessionService sessionService,
        long chatId,
        string text,
        InlineKeyboardMarkup? replyMarkup,
        CancellationToken cancellationToken)
    {
        var session = requestContext.Session as AtlasUserSession;
        var previousMessageId = session?.RoomMenuMessageId;

        var sent = await telegram.SendAndDeleteAsync(
            chatId,
            previousMessageId,
            text,
            replyMarkup,
            cancellationToken);

        if (session is null)
        {
            return;
        }

        session.RoomMenuMessageId = sent.Id;

        await sessionService.SaveSession(session, cancellationToken);
    }
}
