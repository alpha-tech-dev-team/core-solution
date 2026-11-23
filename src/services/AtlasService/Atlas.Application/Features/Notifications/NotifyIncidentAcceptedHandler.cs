using Atlas.Domain.Constants.Localization;
using CoreSolution.Shared.Abstractions;
using CoreSolution.Shared.Results;
using MediatR;

namespace Atlas.Application.Features.Notifications;

public sealed class NotifyIncidentAcceptedHandler : IRequestHandler<NotifyIncidentAcceptedMessage, Result>
{
    private readonly IncidentNotificationDispatcher _notifications;
    private readonly ILocalizer _localizer;

    public NotifyIncidentAcceptedHandler(
        IncidentNotificationDispatcher notifications,
        ILocalizer localizer)
    {
        _notifications = notifications;
        _localizer = localizer;
    }

    public async Task<Result> Handle(NotifyIncidentAcceptedMessage request, CancellationToken cancellationToken)
    {
        var incident = request.Incident;
        var text = string.Format(
            _localizer.GetPhrase(
                nameof(LocalizationKeysConstants.IncidentAccepted),
                LocalizationKeysConstants.IncidentAccepted.Text),
            incident.TicketNumber);

        await _notifications.SendAsync(incident, text, cancellationToken);

        return Result.Success();
    }
}
