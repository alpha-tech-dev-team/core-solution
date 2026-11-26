using Atlas.Domain.Constants.Localization;
using Atlas.Domain.Enums;
using Atlas.Domain.Interfaces.Services;
using CoreSolution.Shared.Abstractions;
using CoreSolution.Shared.Results;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Atlas.Application.Features.Notifications;

public sealed class NotifyIncidentHandler : IRequestHandler<NotifyIncidentMessage, Result>
{
    private readonly IIncidentNotifier _notifier;
    private readonly ILocalizer _localizer;
    private readonly ILogger<NotifyIncidentHandler> _logger;

    public NotifyIncidentHandler(
        IIncidentNotifier notifier,
        ILocalizer localizer,
        ILogger<NotifyIncidentHandler> logger)
    {
        _notifier = notifier;
        _localizer = localizer;
        _logger = logger;
    }

    public async Task<Result> Handle(NotifyIncidentMessage request, CancellationToken cancellationToken)
    {
        var incident = request.Incident;
        var recipients = new[] { incident.Employee?.TelegramUserId, incident.AssignedEngineer?.TelegramUserId }
            .OfType<long>()
            .Distinct()
            .ToList();

        if (recipients.Count == 0)
        {
            _logger.LogInformation("Для заявки {TicketNumber} нет получателей Telegram", incident.TicketNumber);
            return Result.Success();
        }

        var text = FormatText(request);
        var deliveryFailed = false;

        foreach (var chatId in recipients)
        {
            try
            {
                await _notifier.SendAsync(chatId, text, cancellationToken);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception exception)
            {
                deliveryFailed = true;
                _logger.LogWarning(exception, "Не удалось отправить уведомление по заявке {TicketNumber} в чат {ChatId}", incident.TicketNumber, chatId);
            }
        }

        return deliveryFailed
            ? Result.Failure(nameof(ErrorCode.NotificationDeliveryFailed))
            : Result.Success();
    }

    private string FormatText(NotifyIncidentMessage request)
    {
        var incident = request.Incident;

        return request.Type switch
        {
            IncidentNotificationType.Created => string.Format(
                _localizer.GetPhrase(nameof(LocalizationKeysConstants.IncidentCreatedNotification), LocalizationKeysConstants.IncidentCreatedNotification.Text),
                incident.TicketNumber),
            IncidentNotificationType.Accepted => string.Format(
                _localizer.GetPhrase(nameof(LocalizationKeysConstants.IncidentAccepted), LocalizationKeysConstants.IncidentAccepted.Text),
                incident.TicketNumber),
            IncidentNotificationType.Resolved => string.Format(
                _localizer.GetPhrase(nameof(LocalizationKeysConstants.IncidentResolved), LocalizationKeysConstants.IncidentResolved.Text),
                incident.TicketNumber,
                incident.Resolution ?? string.Empty),
            _ => throw new ArgumentOutOfRangeException(nameof(request), request.Type, "Unknown incident notification type")
        };
    }
}
