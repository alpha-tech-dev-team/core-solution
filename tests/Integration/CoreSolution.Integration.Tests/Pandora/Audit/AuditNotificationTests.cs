using System.Collections.Concurrent;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Pandora.Domain.Entities;
using Pandora.Domain.Interfaces;
using Pandora.Domain.Models;
using Pandora.Infrastructure.Audit;
using Pandora.Infrastructure.Database;
using Xunit;

namespace CoreSolution.Integration.Tests.PandoraTests;

[Collection(nameof(IntegrationCollection))]
public sealed class AuditNotificationTests(IntegrationFixture fixture)
{
    [Fact]
    public async Task SaveChangesAsync_AfterAuditEnqueued_WritesOutboxEntryInSameTransaction()
    {
        var auditContext = new AuditContext();
        await using var database = CreateDatabase(auditContext, writeOutbox: true);
        var entry = CreateEntry();
        auditContext.Enqueue(entry);

        await database.SaveChangesAsync();

        Assert.True(await database.AuditEntry.AnyAsync(item => item.Id == entry.Id));
        var outbox = Assert.Single(await FindOutboxAsync(entry.Id));
        var notification = AuditNotificationSerializer.Deserialize(outbox.Payload);
        Assert.NotNull(notification);
        Assert.Equal(entry.Id, notification.Id);
        Assert.Equal(entry.UserId, notification.UserId);
        Assert.Equal(AuditNotificationSerializer.Serialize(AuditEntryNotification.From(entry)), outbox.Payload);
    }

    [Fact]
    public async Task SaveChangesAsync_WhenOutboxDisabled_WritesAuditEntryOnly()
    {
        var auditContext = new AuditContext();
        await using var database = CreateDatabase(auditContext, writeOutbox: false);
        var entry = CreateEntry();
        auditContext.Enqueue(entry);

        await database.SaveChangesAsync();

        Assert.True(await database.AuditEntry.AnyAsync(item => item.Id == entry.Id));
        Assert.Empty(await FindOutboxAsync(entry.Id));
    }

    [Fact]
    public void SaveChanges_AfterAuditEnqueued_WritesOutboxEntry()
    {
        var auditContext = new AuditContext();
        using var database = CreateDatabase(auditContext, writeOutbox: true);
        var entry = CreateEntry();
        auditContext.Enqueue(entry);

        database.SaveChanges();

        Assert.True(database.AuditEntry.Any(item => item.Id == entry.Id));
        Assert.Single(FindOutbox(entry.Id));
    }

    [Fact]
    public async Task SaveChanges_WhenCommitFails_WritesNoOutboxEntry()
    {
        var auditContext = new AuditContext();
        await using var database = CreateDatabase(auditContext, writeOutbox: true);
        var validEntry = CreateEntry();
        var invalidEntry = CreateEntry(action: new string('a', 500));
        auditContext.Enqueue(validEntry);
        auditContext.Enqueue(invalidEntry);

        await Assert.ThrowsAsync<DbUpdateException>(() => database.SaveChangesAsync());

        Assert.Empty(await FindOutboxAsync(validEntry.Id));
        Assert.Empty(await FindOutboxAsync(invalidEntry.Id));
    }

    [Fact]
    public async Task Publisher_PublishesOutboxEntryAndRemovesIt()
    {
        var action = $"outbox_test_{Guid.NewGuid():N}";
        var auditContext = new AuditContext();
        await using var database = CreateDatabase(auditContext, writeOutbox: true);
        var entry = CreateEntry(action: action);
        auditContext.Enqueue(entry);
        await database.SaveChangesAsync();
        Assert.Single(await FindOutboxAsync(entry.Id));

        var notifier = new RecordingAuditNotifier();
        await using var provider = CreatePublisherServices(notifier);
        var publisher = new AuditOutboxPublisher(
            provider.GetRequiredService<IServiceScopeFactory>(),
            notifier,
            NullLogger<AuditOutboxPublisher>.Instance);
        using var cancellation = new CancellationTokenSource();
        await publisher.StartAsync(cancellation.Token);

        try
        {
            await notifier.WaitForNotificationAsync(action).WaitAsync(TimeSpan.FromSeconds(10));
            await WaitForOutboxRemovalAsync(entry.Id);
        }
        finally
        {
            await publisher.StopAsync(CancellationToken.None);
        }

        var publishedSnapshot = notifier.Published.ToArray();
        var published = Assert.Single(publishedSnapshot, item => item.Action == action);
        Assert.Equal(entry.Id, published.Id);
    }

    [Fact]
    public async Task Publisher_RemovesMalformedPayloadAndPublishesValidEntry()
    {
        var action = $"outbox_malformed_{Guid.NewGuid():N}";
        var auditContext = new AuditContext();
        await using var database = CreateDatabase(auditContext, writeOutbox: true);
        var entry = CreateEntry(action: action);
        auditContext.Enqueue(entry);
        await database.SaveChangesAsync();

        var malformedId = Guid.NewGuid();
        await using (var writer = CreateReader())
        {
            writer.AuditOutbox.Add(new AuditOutboxEntry(malformedId, "{not-json", DateTime.UtcNow));
            await writer.SaveChangesAsync();
        }

        var notifier = new RecordingAuditNotifier();
        await using var provider = CreatePublisherServices(notifier);
        var publisher = new AuditOutboxPublisher(
            provider.GetRequiredService<IServiceScopeFactory>(),
            notifier,
            NullLogger<AuditOutboxPublisher>.Instance);
        using var cancellation = new CancellationTokenSource();
        await publisher.StartAsync(cancellation.Token);

        try
        {
            await notifier.WaitForNotificationAsync(action).WaitAsync(TimeSpan.FromSeconds(10));
            await WaitForOutboxRemovalAsync(entry.Id);
        }
        finally
        {
            await publisher.StopAsync(CancellationToken.None);
        }

        Assert.Null(await FindOutboxRowAsync(malformedId));
    }

    private PandoraDbContext CreateDatabase(IAuditContext auditContext, bool writeOutbox)
    {
        var options = new DbContextOptionsBuilder<PandoraDbContext>()
            .UseNpgsql(fixture.PandoraConnectionString)
            .AddInterceptors(new AuditSaveChangesInterceptor(auditContext, writeOutbox))
            .Options;

        return new PandoraDbContext(options);
    }

    private ServiceProvider CreatePublisherServices(IAuditNotifier notifier)
    {
        var services = new ServiceCollection();

        services.AddLogging();
        services.AddDbContext<PandoraDbContext>(options => options.UseNpgsql(fixture.PandoraConnectionString));
        services.AddSingleton(notifier);

        return services.BuildServiceProvider();
    }

    private async Task<IReadOnlyList<AuditOutboxEntry>> FindOutboxAsync(Guid entryId)
    {
        await using var database = CreateReader();

        return await database.AuditOutbox.AsNoTracking()
            .Where(item => item.Payload.Contains(entryId.ToString()))
            .ToListAsync();
    }

    private IReadOnlyList<AuditOutboxEntry> FindOutbox(Guid entryId)
    {
        using var database = CreateReader();

        return database.AuditOutbox.AsNoTracking()
            .Where(item => item.Payload.Contains(entryId.ToString()))
            .ToList();
    }

    private PandoraDbContext CreateReader()
    {
        return new PandoraDbContext(new DbContextOptionsBuilder<PandoraDbContext>()
            .UseNpgsql(fixture.PandoraConnectionString)
            .Options);
    }

    private async Task<AuditOutboxEntry?> FindOutboxRowAsync(Guid id)
    {
        await using var database = CreateReader();

        return await database.AuditOutbox.AsNoTracking().SingleOrDefaultAsync(item => item.Id == id);
    }

    private async Task WaitForOutboxRemovalAsync(Guid entryId)
    {
        for (var attempt = 0; attempt < 100; attempt++)
        {
            if (await FindOutboxAsync(entryId) is { Count: 0 })
            {
                return;
            }

            await Task.Delay(50);
        }

        Assert.Fail("Outbox-запись не удалена publisher-ом");
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

    private sealed class RecordingAuditNotifier : IAuditNotifier
    {
        private readonly ConcurrentDictionary<string, TaskCompletionSource> _notified = new();
        private readonly ConcurrentQueue<AuditEntryNotification> _published = new();

        public ConcurrentQueue<AuditEntryNotification> Published => _published;

        public Task Notify(AuditEntryNotification notification)
        {
            _published.Enqueue(notification);
            _notified.GetOrAdd(notification.Action, _ => new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously))
                .TrySetResult();

            return Task.CompletedTask;
        }

        public Task WaitForNotificationAsync(string action)
        {
            return _notified.GetOrAdd(action, _ => new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously)).Task;
        }
    }
}
