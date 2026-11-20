using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Pandora.Domain.Entities;
using Pandora.Domain.Interfaces;
using Pandora.Domain.Models;
using Pandora.Infrastructure.Audit;

namespace Pandora.Infrastructure.Database;

public sealed class AuditSaveChangesInterceptor : SaveChangesInterceptor
{
    private readonly IAuditContext _auditContext;
    private readonly bool _writeOutbox;

    public AuditSaveChangesInterceptor(IAuditContext auditContext, bool writeOutbox)
    {
        _auditContext = auditContext;
        _writeOutbox = writeOutbox;
    }

    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        AddPendingAudit(eventData.Context);

        return base.SavingChanges(eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        AddPendingAudit(eventData.Context);

        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private void AddPendingAudit(DbContext? context)
    {
        if (context is null)
        {
            return;
        }

        foreach (var entry in _auditContext.TakeAll())
        {
            context.Add(entry);

            if (_writeOutbox)
            {
                context.Add(new AuditOutboxEntry(
                    Guid.NewGuid(),
                    AuditNotificationSerializer.Serialize(AuditEntryNotification.From(entry)),
                    DateTime.UtcNow));
            }
        }
    }
}
