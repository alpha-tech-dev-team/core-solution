using Atlas.Application.Abstractions;
using Atlas.Domain.Constants.Localization;
using CoreSolution.Shared.Abstractions;
using CoreSolution.Shared.Results;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Atlas.Application.Features.Notifications;

public sealed class NotifyIncidentAcceptedHandler : IRequestHandler<NotifyIncidentAcceptedMessage, Result>
{
    private readonly ITelegramMessageSender _sender;
    private readonly ILocalizer _localizer;
    private readonly ILogger<NotifyIncidentAcceptedHandler> _logger;

    public NotifyIncidentAcceptedHandler(
        ITelegramMessageSender sender,
        ILocalizer localizer,
        ILogger<NotifyIncidentAcceptedHandler> logger)
    {
        _sender = sender;
        _localizer = localizer;
        _logger = logger;
    }

    public async Task<Result> Handle(NotifyIncidentAcceptedMessage request, CancellationToken cancellationToken)
    {
        var incident = request.Incident;
        var chatIds = IncidentRecipients.From(incident);

        if (chatIds.Count == 0)
        {
            _logger.LogDebug("Для заявки {TicketNumber} нет получателей Telegram", incident.TicketNumber);
            return Result.Success();
        }

        var text = string.Format(
            _localizer.GetPhrase(
                nameof(LocalizationKeysConstants.IncidentAccepted),
                LocalizationKeysConstants.IncidentAccepted.Text),
            incident.TicketNumber);

        await _sender.SendAsync(chatIds, text, cancellationToken);

        return Result.Success();
    }
}
