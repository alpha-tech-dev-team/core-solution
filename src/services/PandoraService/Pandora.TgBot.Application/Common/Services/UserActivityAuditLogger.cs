using Microsoft.Extensions.Logging;
using Pandora.Domain.Constants;
using Pandora.Domain.Entities;
using Pandora.Domain.Interfaces;
using Pandora.TgBot.Application.Common.Interfaces;

namespace Pandora.TgBot.Application.Common.Services;

public sealed class UserActivityAuditLogger
{
    private readonly IAuditContext _auditContext;
    private readonly IAuditSaver _auditSaver;
    private readonly ILogger<UserActivityAuditLogger> _logger;

    public UserActivityAuditLogger(
        IAuditContext auditContext,
        IAuditSaver auditSaver,
        ILogger<UserActivityAuditLogger> logger)
    {
        _auditContext = auditContext;
        _auditSaver = auditSaver;
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
            _auditContext.Enqueue(entry);
            await _auditSaver.SaveAsync(cancellationToken);
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            _logger.LogError(exception, "Не удалось записать действие {Action} пользователя {TgUserId}", action, telegramUserId);
        }
    }
}
