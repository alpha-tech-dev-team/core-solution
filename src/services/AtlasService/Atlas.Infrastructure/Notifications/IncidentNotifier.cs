using Atlas.Domain.Interfaces.Services;
using CoreSolution.Shared.TgBot.Abstractions;

namespace Atlas.Infrastructure.Notifications;

public sealed class IncidentNotifier : IIncidentNotifier
{
    private readonly ITelegramNotificationService _notifications;

    public IncidentNotifier(ITelegramNotificationService notifications)
    {
        _notifications = notifications;
    }

    public Task SendAsync(long chatId, string text, CancellationToken cancellationToken = default)
    {
        return _notifications.SendAlertAsync(chatId, text, cancellationToken: cancellationToken);
    }
}
