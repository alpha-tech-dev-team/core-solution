using Atlas.Application.Abstractions;
using Atlas.Domain.Constants.Localization;
using CoreSolution.Shared.Abstractions;
using CoreSolution.Shared.Results;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Atlas.Application.Features.Notifications;

public sealed class NotifyIncidentResolvedHandler : IRequestHandler<NotifyIncidentResolvedMessage, Result>
{
    private readonly ITelegramMessageSender _sender;
    private readonly ILocalizer _localizer;
    private readonly ILogger<NotifyIncidentResolvedHandler> _logger;

    public NotifyIncidentResolvedHandler(
        ITelegramMessageSender sender,
        ILocalizer localizer,
        ILogger<NotifyIncidentResolvedHandler> logger)
    {
        _sender = sender;
        _localizer = localizer;
        _logger = logger;
    }

    public async Task<Result> Handle(NotifyIncidentResolvedMessage request, CancellationToken cancellationToken)
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
                nameof(LocalizationKeysConstants.IncidentResolved),
                LocalizationKeysConstants.IncidentResolved.Text),
            incident.TicketNumber,
            incident.Resolution ?? string.Empty);

        await _sender.SendAsync(chatIds, text, cancellationToken);

        return Result.Success();
    }
}
