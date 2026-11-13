using Pandora.Domain.Constants;
using Pandora.Domain.Entities;
using Pandora.Domain.Interfaces;

namespace Pandora.Application.Features.Secrets.Common;

public sealed class SecretAuditLogger
{
    private readonly IAuditContext _auditContext;

    public SecretAuditLogger(IAuditContext auditContext)
    {
        _auditContext = auditContext;
    }

    public Task LogActionAsync(Guid adminUserId, string action, string targetItemId, CancellationToken cancellationToken)
    {
        _auditContext.Enqueue(
            new AuditEntry(
                Guid.NewGuid(),
                DateTime.UtcNow,
                adminUserId,
                action,
                targetItemId,
                AuditConstants.SourceWeb));

        return Task.CompletedTask;
    }
}
