using CoreSolution.Shared.TgBot.Abstractions;
using Pandora.Domain.Interfaces;

namespace Pandora.TgBot.Credentials;

public sealed class TelegramCredentialMessageNotifier : ICredentialMessageNotifier
{
    private readonly ITelegramNotificationService _notificationService;

    public TelegramCredentialMessageNotifier(ITelegramNotificationService notificationService)
    {
        _notificationService = notificationService;
    }

    public async Task SendNotice(long chatId, string text, CancellationToken cancellationToken)
    {
        await _notificationService.SendMessageAsync(chatId, text, null, cancellationToken);
    }
}
