using Atlas.Application.Abstractions;
using Atlas.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace Atlas.Application.Features.Notifications;

public sealed class IncidentNotificationDispatcher
{
    private readonly ITelegramMessageSender _sender;
    private readonly ILogger<IncidentNotificationDispatcher> _logger;

    public IncidentNotificationDispatcher(
        ITelegramMessageSender sender,
        ILogger<IncidentNotificationDispatcher> logger)
    {
        _sender = sender;
        _logger = logger;
    }

    public async Task SendAsync(Incident incident, string text, CancellationToken cancellationToken)
    {
        var chatIds = IncidentRecipients.From(incident);

        if (chatIds.Count == 0)
        {
            _logger.LogInformation("Для заявки {TicketNumber} нет получателей Telegram", incident.TicketNumber);
            return;
        }

        await _sender.SendAsync(chatIds, text, cancellationToken);
    }
}
