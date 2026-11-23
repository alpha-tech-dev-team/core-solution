using Atlas.Domain.Constants.Localization;
using CoreSolution.Shared.Abstractions;
using CoreSolution.Shared.Results;
using MediatR;

namespace Atlas.Application.Features.Notifications;

public sealed class NotifyIncidentResolvedHandler : IRequestHandler<NotifyIncidentResolvedMessage, Result>
{
    private readonly IncidentNotificationDispatcher _notifications;
    private readonly ILocalizer _localizer;

    public NotifyIncidentResolvedHandler(
        IncidentNotificationDispatcher notifications,
        ILocalizer localizer)
    {
        _notifications = notifications;
        _localizer = localizer;
    }

    public async Task<Result> Handle(NotifyIncidentResolvedMessage request, CancellationToken cancellationToken)
    {
        var incident = request.Incident;
        var text = string.Format(
            _localizer.GetPhrase(
                nameof(LocalizationKeysConstants.IncidentResolved),
                LocalizationKeysConstants.IncidentResolved.Text),
            incident.TicketNumber,
            incident.Resolution ?? string.Empty);

        await _notifications.SendAsync(incident, text, cancellationToken);

        return Result.Success();
    }
}
