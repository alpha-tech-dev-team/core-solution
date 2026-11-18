using Microsoft.Extensions.Logging;
using Pandora.Domain.Constants;
using Pandora.Domain.Entities;
using Pandora.Domain.Interfaces;
using Pandora.TgBot.Application.Common.Interfaces;

namespace Pandora.TgBot.Application.Common.Services;

public sealed class UserActivityAuditLogger
{
    private readonly IAuditRepository _auditRepository;
    private readonly IAuditNotifier _auditNotifier;
    private readonly ILogger<UserActivityAuditLogger> _logger;

    public UserActivityAuditLogger(
        IAuditRepository auditRepository,
        IAuditNotifier auditNotifier,
        ILogger<UserActivityAuditLogger> logger)
    {
        _auditRepository = auditRepository;
        _auditNotifier = auditNotifier;
        _logger = logger;
    }

    public Task Log(IUserActionMessage userAction, CancellationToken cancellationToken)
    {
        return Log(userAction.TelegramUserId, userAction.Action, userAction.TargetItemId, cancellationToken);
    }

    public async Task Log(long telegramUserId, string action, string? targetItemId, CancellationToken cancellationToken)
    {
        var entry = new AuditEntry(Guid.NewGuid(), DateTime.UtcNow, telegramUserId, action, targetItemId, AuditConstants.Sources.Telegram);

        try
        {
            await _auditRepository.Add(entry, cancellationToken);
            await _auditNotifier.NotifyAsync(entry);
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            _logger.LogError(exception, "Не удалось записать действие {Action} пользователя {TgUserId}", action, telegramUserId);
        }
    }
}
