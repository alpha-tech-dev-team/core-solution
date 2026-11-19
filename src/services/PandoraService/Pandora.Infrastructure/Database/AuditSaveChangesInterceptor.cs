using System.Runtime.CompilerServices;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Pandora.Domain.Entities;
using Pandora.Domain.Interfaces;

namespace Pandora.Infrastructure.Database;

public sealed class AuditSaveChangesInterceptor : SaveChangesInterceptor
{
    private readonly IAuditContext _auditContext;
    private readonly IAuditNotifier? _auditNotifier;
    private readonly ConditionalWeakTable<DbContext, List<AuditEntry>> _pendingByContext = new();

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

    public override int SavedChanges(SaveChangesCompletedEventData eventData, int result)
    {
        PublishAfterCommit(eventData.Context);

        return base.SavedChanges(eventData, result);
    }

    public override async ValueTask<int> SavedChangesAsync(
        SaveChangesCompletedEventData eventData,
        int result,
        CancellationToken cancellationToken = default)
    {
        await PublishAfterCommitAsync(eventData.Context);

        return await base.SavedChangesAsync(eventData, result, cancellationToken);
    }

    public override void SaveChangesFailed(DbContextErrorEventData eventData)
    {
        RemovePending(eventData.Context);

        base.SaveChangesFailed(eventData);
    }

    public override Task SaveChangesFailedAsync(
        DbContextErrorEventData eventData,
        CancellationToken cancellationToken = default)
    {
        RemovePending(eventData.Context);

        return base.SaveChangesFailedAsync(eventData, cancellationToken);
    }

    public override void SaveChangesCanceled(DbContextEventData eventData)
    {
        RemovePending(eventData.Context);

        base.SaveChangesCanceled(eventData);
    }

    public override Task SaveChangesCanceledAsync(
        DbContextEventData eventData,
        CancellationToken cancellationToken = default)
    {
        RemovePending(eventData.Context);

        return base.SaveChangesCanceledAsync(eventData, cancellationToken);
    }

    private void AddAuditEntries(DbContext? context)
    {
        if (context is null)
        {
            return;
        }

        var pending = _pendingByContext.GetOrCreateValue(context);

        foreach (var entry in _auditContext.TakeAll())
        {
            context.Add(entry);
            pending.Add(entry);
        }
    }

    private void PublishAfterCommit(DbContext? context)
    {
        if (_auditNotifier is null)
        {
            RemovePending(context);

            return;
        }

        foreach (var entry in TakePending(context))
        {
            _auditNotifier.Notify(entry).GetAwaiter().GetResult();
        }
    }

    private async Task PublishAfterCommitAsync(DbContext? context)
    {
        var pending = TakePending(context);
        if (_auditNotifier is null)
        {
            return;
        }

        foreach (var entry in pending)
        {
            await _auditNotifier.Notify(entry);
        }
    }

    private IReadOnlyList<AuditEntry> TakePending(DbContext? context)
    {
        if (context is null || !_pendingByContext.TryGetValue(context, out var pending))
        {
            return [];
        }

        _pendingByContext.Remove(context);

        return pending;
    }

    private void RemovePending(DbContext? context)
    {
        if (context is not null)
        {
            _pendingByContext.Remove(context);
        }
    }
}
