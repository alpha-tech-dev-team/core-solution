using Atlas.Application.Abstractions;
using Atlas.Domain.Constants.Localization;
using Atlas.Domain.Entities;
using CoreSolution.Shared.Abstractions;
using CoreSolution.Shared.TgBot.Abstractions;
using Microsoft.Extensions.Logging;

namespace Atlas.TgBot.Notifications;

public sealed class IncidentNotificationService : IIncidentNotificationService
{
    private readonly ITelegramNotificationService _telegramNotificationService;
    private readonly ILocalizer _localizer;
    private readonly ILogger<IncidentNotificationService> _logger;

    public IncidentNotificationService(
        ITelegramNotificationService telegramNotificationService,
        ILocalizer localizer,
        ILogger<IncidentNotificationService> logger)
    {
        _telegramNotificationService = telegramNotificationService;
        _localizer = localizer;
        _logger = logger;
    }

    public async Task NotifyAcceptedAsync(Incident incident, CancellationToken cancellationToken = default)
    {
        var text = string.Format(
            _localizer.GetPhrase(
                nameof(LocalizationKeysConstants.IncidentAccepted),
                LocalizationKeysConstants.IncidentAccepted.Text),
            incident.TicketNumber);

        await NotifyAsync(incident, text, cancellationToken);
    }

    public async Task NotifyResolvedAsync(Incident incident, CancellationToken cancellationToken = default)
    {
        var text = string.Format(
            _localizer.GetPhrase(
                nameof(LocalizationKeysConstants.IncidentResolved),
                LocalizationKeysConstants.IncidentResolved.Text),
            incident.TicketNumber,
            incident.Resolution ?? string.Empty);

        await NotifyAsync(incident, text, cancellationToken);
    }

    private async Task NotifyAsync(Incident incident, string text, CancellationToken cancellationToken)
    {
        var chatIds = new[]
            {
                incident.Employee.TelegramUserId,
                incident.AssignedEngineer?.TelegramUserId
            }
            .Where(chatId => chatId.HasValue)
            .Select(chatId => chatId.GetValueOrDefault())
            .Distinct()
            .ToArray();

        if (chatIds.Length == 0)
        {
            _logger.LogDebug(
                "Для заявки {IncidentId} (№{TicketNumber}) нет получателей Telegram",
                incident.Id,
                incident.TicketNumber);

            return;
        }

        foreach (var chatId in chatIds)
        {
            try
            {
                await _telegramNotificationService.SendAlertAsync(
                    chatId,
                    text,
                    cancellationToken: cancellationToken);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception exception)
            {
                _logger.LogError(
                    exception,
                    "Не удалось отправить уведомление по заявке {IncidentId} (№{TicketNumber}) в чат {ChatId}",
                    incident.Id,
                    incident.TicketNumber,
                    chatId);
            }
        }
    }
}
