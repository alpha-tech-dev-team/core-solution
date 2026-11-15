using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Pandora.Domain.Entities;
using Pandora.Infrastructure.Database;
using Pandora.Infrastructure.Repositories;
using Xunit;

namespace CoreSolution.Integration.Tests.PandoraTests;

[Collection(nameof(IntegrationCollection))]
public sealed class AuditRepositoryTests(IntegrationFixture fixture)
{
    [Fact]
    public async Task GetPage_ReturnsNewestFirstWithTotalCount()
    {
        await using var provider = await fixture.CreatePandoraServiceProviderAsync();
        using var scope = provider.CreateScope();
        var database = scope.ServiceProvider.GetRequiredService<PandoraDbContext>();
        var repository = new AuditRepository(database);

        await database.AuditEntry.ExecuteDeleteAsync();

        var oldest = CreateEntry(new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc), "first");
        var middle = CreateEntry(new DateTime(2026, 1, 2, 0, 0, 0, DateTimeKind.Utc), "second");
        var newest = CreateEntry(new DateTime(2026, 1, 3, 0, 0, 0, DateTimeKind.Utc), "third");

        database.AuditEntry.AddRange(oldest, middle, newest);
        await database.SaveChangesAsync();

        var firstPage = await repository.GetPage(1, 2, CancellationToken.None);

        Assert.Equal(3, firstPage.Total);
        Assert.Equal(1, firstPage.Page);
        Assert.Equal(2, firstPage.PageSize);
        Assert.Equal([newest.Id, middle.Id], firstPage.Items.Select(entry => entry.Id));

        var secondPage = await repository.GetPage(2, 2, CancellationToken.None);

        Assert.Equal(3, secondPage.Total);
        Assert.Equal([oldest.Id], secondPage.Items.Select(entry => entry.Id));
    }

    [Fact]
    public async Task Add_PersistsWebAdminEntryWithUserIdAndNullTelegramUserId()
    {
        await using var provider = await fixture.CreatePandoraServiceProviderAsync();
        using var scope = provider.CreateScope();
        var database = scope.ServiceProvider.GetRequiredService<PandoraDbContext>();
        var repository = new AuditRepository(database);
        var adminId = Guid.NewGuid();

        await database.AuditEntry.ExecuteDeleteAsync();

        await repository.Add(
            new AuditEntry(Guid.NewGuid(), DateTime.UtcNow, adminId, "secret_created", null, "web"),
            CancellationToken.None);

        var stored = await database.AuditEntry.AsNoTracking().SingleAsync();

        Assert.Equal(adminId, stored.UserId);
        Assert.Null(stored.TelegramUserId);
    }

    [Fact]
    public async Task Add_PersistsTelegramEntryWithTelegramUserIdAndNullUserId()
    {
        await using var provider = await fixture.CreatePandoraServiceProviderAsync();
        using var scope = provider.CreateScope();
        var database = scope.ServiceProvider.GetRequiredService<PandoraDbContext>();
        var repository = new AuditRepository(database);

        await database.AuditEntry.ExecuteDeleteAsync();

        await repository.Add(
            new AuditEntry(Guid.NewGuid(), DateTime.UtcNow, 8201, "show_services", null, "telegram"),
            CancellationToken.None);

        var stored = await database.AuditEntry.AsNoTracking().SingleAsync();

        Assert.Equal(8201, stored.TelegramUserId);
        Assert.Null(stored.UserId);
    }

    private static AuditEntry CreateEntry(DateTime timestamp, string action) =>
        new(Guid.NewGuid(), timestamp, telegramUserId: 0, action, targetItemId: null, requestSource: "telegram");
}
