using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Pandora.Domain.Entities;
using Pandora.Domain.Interfaces;
using Pandora.Infrastructure.Database;
using Xunit;

namespace CoreSolution.Integration.Tests.PandoraTests;

[Collection(nameof(IntegrationCollection))]
public sealed class AuditNotificationTests(IntegrationFixture fixture)
{
    [Fact]
    public async Task SaveChanges_AfterAuditEnqueued_PublishesEntryAfterCommit()
    {
        var notifier = new RecordingAuditNotifier();
        var auditContext = new AuditContext();
        await using var database = CreateDatabase(auditContext, notifier);
        var entry = CreateEntry();
        auditContext.Enqueue(entry);

        await database.SaveChangesAsync();

        var published = Assert.Single(notifier.Published);
        Assert.Equal(entry.Id, published.Id);
        Assert.Equal(entry.UserId, published.UserId);
        Assert.True(await database.AuditEntry.AnyAsync(item => item.Id == entry.Id));
    }

    [Fact]
    public void SaveChangesSync_AfterAuditEnqueued_PublishesEntryAfterCommit()
    {
        var notifier = new RecordingAuditNotifier();
        var auditContext = new AuditContext();
        using var database = CreateDatabase(auditContext, notifier);
        var entry = CreateEntry();
        auditContext.Enqueue(entry);

        database.SaveChanges();

        var published = Assert.Single(notifier.Published);
        Assert.Equal(entry.Id, published.Id);
        Assert.True(database.AuditEntry.Any(item => item.Id == entry.Id));
    }

    [Fact]
    public async Task SaveChangesSync_WithSlowNotifier_DoesNotWaitForPublication()
    {
        var notifier = new BlockingAuditNotifier();
        var auditContext = new AuditContext();
        using var database = CreateDatabase(auditContext, notifier);
        var entry = CreateEntry();
        auditContext.Enqueue(entry);

        database.SaveChanges();

        Assert.Empty(notifier.Published);

        notifier.Release();
        await notifier.WaitForPublished().WaitAsync(TimeSpan.FromSeconds(5));

        Assert.Single(notifier.Published);
    }

    [Fact]
    public async Task SaveChanges_WhenCommitFails_PublishesNothing()
    {
        var notifier = new RecordingAuditNotifier();
        var auditContext = new AuditContext();
        await using var database = CreateDatabase(auditContext, notifier);
        var entry = CreateEntry();
        auditContext.Enqueue(entry);
        auditContext.Enqueue(CreateEntry(action: new string('a', 500)));

        await Assert.ThrowsAsync<DbUpdateException>(() => database.SaveChangesAsync());

        Assert.Empty(notifier.Published);

        database.ChangeTracker.Clear();
        await database.SaveChangesAsync();

        Assert.Empty(notifier.Published);
    }

    private PandoraDbContext CreateDatabase(IAuditContext auditContext, IAuditNotifier notifier)
    {
        var options = new DbContextOptionsBuilder<PandoraDbContext>()
            .UseNpgsql(fixture.PandoraConnectionString)
            .AddInterceptors(new AuditSaveChangesInterceptor(
                auditContext,
                NullLogger<AuditSaveChangesInterceptor>.Instance,
                notifier))
            .Options;

        return new PandoraDbContext(options);
    }

    private static AuditEntry CreateEntry(Guid? id = null, string action = "secret_created")
    {
        return new AuditEntry(
            id ?? Guid.NewGuid(),
            DateTime.UtcNow,
            Guid.NewGuid(),
            action,
            "test-service",
            "web");
    }

    private sealed class BlockingAuditNotifier : IAuditNotifier
    {
        private readonly TaskCompletionSource _gate = new(TaskCreationOptions.RunContinuationsAsynchronously);
        private readonly TaskCompletionSource _published = new(TaskCreationOptions.RunContinuationsAsynchronously);

        public List<AuditEntry> Published { get; } = [];

        public Task Notify(AuditEntry entry)
        {
            return WaitAndRecord(entry);
        }

        public void Release()
        {
            _gate.SetResult();
        }

        public Task WaitForPublished()
        {
            return _published.Task;
        }

        private async Task WaitAndRecord(AuditEntry entry)
        {
            await _gate.Task;
            Published.Add(entry);
            _published.SetResult();
        }
    }

    private sealed class RecordingAuditNotifier : IAuditNotifier
    {
        public List<AuditEntry> Published { get; } = [];

        public Task Notify(AuditEntry entry)
        {
            Published.Add(entry);

            return Task.CompletedTask;
        }
    }
}
