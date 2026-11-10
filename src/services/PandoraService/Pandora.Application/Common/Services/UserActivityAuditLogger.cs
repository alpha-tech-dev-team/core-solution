using Microsoft.Extensions.Logging;
using Pandora.Application.Common.Interfaces;
using Pandora.Domain.Constants;
using Pandora.Domain.Entities;
using Pandora.Domain.Interfaces;

namespace Pandora.Application.Common.Services;

public sealed class UserActivityAuditLogger
{
    private readonly IAuditRepository _auditRepository;
    private readonly ILogger<UserActivityAuditLogger> _logger;

    public UserActivityAuditLogger(IAuditRepository auditRepository, ILogger<UserActivityAuditLogger> logger)
    {
        _auditRepository = auditRepository;
        _logger = logger;
    }

    public async Task LogAsync(IUserActionMessage userAction, CancellationToken cancellationToken)
    {
        var entry = new AuditEntry(Guid.NewGuid(), DateTime.UtcNow, userAction.TelegramUserId, userAction.Action, userAction.TargetItemId, AuditConstants.SourceTelegram);

        try
        {
            await _auditRepository.Add(entry, cancellationToken);
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            _logger.LogError(exception, "Не удалось записать действие {Action} пользователя {TgUserId}", userAction.Action, userAction.TelegramUserId);
        }
    }
}
