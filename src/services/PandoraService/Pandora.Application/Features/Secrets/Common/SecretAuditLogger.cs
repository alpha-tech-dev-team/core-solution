using Microsoft.Extensions.Logging;
using Pandora.Domain.Constants;
using Pandora.Domain.Entities;
using Pandora.Domain.Interfaces;

namespace Pandora.Application.Features.Secrets.Common;

public sealed class SecretAuditLogger
{
    private readonly IAuditRepository _auditRepository;
    private readonly ILogger<SecretAuditLogger> _logger;

    public SecretAuditLogger(IAuditRepository auditRepository, ILogger<SecretAuditLogger> logger)
    {
        _auditRepository = auditRepository;
        _logger = logger;
    }

    public async Task LogActionAsync(Guid adminUserId, string action, string targetItemId, CancellationToken cancellationToken)
    {
        try
        {
            await _auditRepository.Add(
                new AuditEntry(
                    Guid.NewGuid(),
                    DateTime.UtcNow,
                    adminUserId,
                    action,
                    targetItemId,
                    AuditConstants.SourceWeb),
                cancellationToken);
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Не удалось записать аудит действия {Action} по секрету {ItemId}", action, targetItemId);
        }
    }
}
