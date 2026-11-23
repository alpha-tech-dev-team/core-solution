using CoreSolution.Shared.Models;
using CoreSolution.Shared.Results;
using CoreSolution.Shared.TgBot.Abstractions;
using CoreSolution.Shared.TgBot.Extensions;
using Pandora.TgBot.Application.Common;
using Telegram.Bot.Types;

namespace Pandora.TgBot.Credentials;

public static class CredentialNotificationExtensions
{
    public static async Task<Message> SendCredentialsAsync(
        this ITelegramNotificationService telegram,
        CredentialMessageScheduler scheduler,
        long chatId,
        Result<WorkflowResponse> result,
        string itemId,
        IUserErrorMapper errorMapper,
        CancellationToken cancellationToken)
    {
        var message = await telegram.SendMessageAsync(
            chatId,
            result.GetUserText(errorMapper),
            result.ToTelegramMarkup(),
            cancellationToken,
            result.ToTelegramSendOptions());

        if (result.HasSensitiveData())
        {
            await scheduler.ScheduleAsync(chatId, message.MessageId, itemId, cancellationToken);
        }

        return message;
    }
}
