using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Pandora.Domain.Entities;
using Pandora.Domain.Interfaces;

namespace Pandora.Infrastructure.Database;

public sealed class AuditSaveChangesInterceptor : SaveChangesInterceptor
{
    private readonly IAuditContext _auditContext;
    private readonly IAuditNotifier? _auditNotifier;
    private readonly List<AuditEntry> _pendingNotifications = [];

    public AuditSaveChangesInterceptor(IAuditContext auditContext, IAuditNotifier? auditNotifier = null)
    {
        _auditContext = auditContext;
        _auditNotifier = auditNotifier;
    }

    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        AddAuditEntries(eventData.Context);

        return base.SavingChanges(eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        AddAuditEntries(eventData.Context);

        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    public override async ValueTask<int> SavedChangesAsync(
        SaveChangesCompletedEventData eventData,
        int result,
        CancellationToken cancellationToken = default)
    {
        if (_auditNotifier is not null)
        {
            foreach (var entry in _pendingNotifications)
            {
                await _auditNotifier.NotifyAsync(entry);
            }
        }

        _pendingNotifications.Clear();

        return await base.SavedChangesAsync(eventData, result, cancellationToken);
    }

    public override Task SaveChangesFailedAsync(
        DbContextErrorEventData eventData,
        CancellationToken cancellationToken = default)
    {
        _pendingNotifications.Clear();

        return base.SaveChangesFailedAsync(eventData, cancellationToken);
    }

    private void AddAuditEntries(DbContext? context)
    {
        if (context is null)
        {
            return;
        }

        foreach (var entry in _auditContext.TakeAll())
        {
            context.Add(entry);
            _pendingNotifications.Add(entry);
        }
    }
}
