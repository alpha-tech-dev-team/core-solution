using CoreSolution.Shared.Pagination;
using Microsoft.Extensions.DependencyInjection;
using Pandora.Domain.Entities;
using Pandora.Domain.Enums;
using Pandora.Domain.Interfaces;
using Pandora.Domain.Models;
using Pandora.Infrastructure.Database;
using Xunit;

namespace CoreSolution.Integration.Tests.PandoraTests;

[Collection(nameof(IntegrationCollection))]
public sealed class EmployeeSecretsRepositoryTests(IntegrationFixture fixture)
{
    [Fact]
    public async Task GetPage_ExcludesSecretsMarkedForDeletion()
    {
        await using var provider = await fixture.CreatePandoraServiceProviderAsync();
        using var scope = provider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<PandoraDbContext>();
        var repository = scope.ServiceProvider.GetRequiredService<IPermissionRepository>();

        var visibleId = NewItemId();
        var deletedId = NewItemId();

        await AddSecret(context, visibleId, SecretSyncOperation.None);
        await AddSecret(context, deletedId, SecretSyncOperation.Delete);

        var page = await repository.GetPage(
            Random.Shared.NextInt64(1, long.MaxValue),
            new SecretPageMessage(
                new PagedMessage { Page = PaginationLimits.DefaultPage, PageSize = PaginationLimits.MaxPageSize },
                OrderByName),
            CancellationToken.None);

        Assert.Contains(page.Items, item => item.Id == visibleId);
        Assert.DoesNotContain(page.Items, item => item.Id == deletedId);
    }

    [Fact]
    public async Task GetPage_ReturnsRequestedSlice()
    {
        await using var provider = await fixture.CreatePandoraServiceProviderAsync();
        using var scope = provider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<PandoraDbContext>();
        var repository = scope.ServiceProvider.GetRequiredService<IPermissionRepository>();

        await AddSecret(context, NewItemId(), SecretSyncOperation.None);
        await AddSecret(context, NewItemId(), SecretSyncOperation.None);

        var firstPage = await repository.GetPage(
            42,
            new SecretPageMessage(
                new PagedMessage { Page = PaginationLimits.FirstPage, PageSize = PaginationLimits.MinPageSize },
                OrderByName),
            CancellationToken.None);

        var secondPage = await repository.GetPage(
            42,
            new SecretPageMessage(
                new PagedMessage { Page = PaginationLimits.FirstPage + 1, PageSize = PaginationLimits.MinPageSize },
                OrderByName),
            CancellationToken.None);

        Assert.Single(firstPage.Items);
        Assert.Equal(PaginationLimits.MinPageSize, firstPage.PageSize);
        Assert.Equal(PaginationLimits.FirstPage, firstPage.Page);
        Assert.Equal(firstPage.Total, secondPage.Total);
        Assert.NotEqual(firstPage.Items[0].Id, secondPage.Items[0].Id);
    }

    [Fact]
    public async Task Grant_NewPermission_SetsGrantedMetadata()
    {
        await using var provider = await fixture.CreatePandoraServiceProviderAsync();
        using var scope = provider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<PandoraDbContext>();
        var repository = scope.ServiceProvider.GetRequiredService<IPermissionRepository>();

        var itemId = NewItemId();
        var telegramUserId = Random.Shared.NextInt64(1, long.MaxValue);
        var adminUserId = Guid.NewGuid();

        await AddSecret(context, itemId, SecretSyncOperation.None);

        var result = await repository.Grant(telegramUserId, itemId, adminUserId, CancellationToken.None);
        await repository.SaveChanges(CancellationToken.None);

        var permission = context.Permission.Single(
            item => item.TelegramUserId == telegramUserId && item.VaultItemId == itemId);

        Assert.Equal(SecretAccessGrantResult.Granted, result);
        Assert.True(permission.IsActive);
        Assert.NotNull(permission.GrantedAt);
        Assert.Equal(adminUserId, permission.GrantedBy);
        Assert.True(await repository.HasAccess(telegramUserId, itemId, CancellationToken.None));
    }

    [Fact]
    public async Task Grant_RepeatedCall_ReportsAlreadyGranted()
    {
        await using var provider = await fixture.CreatePandoraServiceProviderAsync();
        using var scope = provider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<PandoraDbContext>();
        var repository = scope.ServiceProvider.GetRequiredService<IPermissionRepository>();

        var itemId = NewItemId();
        var telegramUserId = Random.Shared.NextInt64(1, long.MaxValue);

        await AddSecret(context, itemId, SecretSyncOperation.None);

        await repository.Grant(telegramUserId, itemId, Guid.NewGuid(), CancellationToken.None);
        await repository.SaveChanges(CancellationToken.None);

        var repeated = await repository.Grant(telegramUserId, itemId, Guid.NewGuid(), CancellationToken.None);

        Assert.Equal(SecretAccessGrantResult.AlreadyGranted, repeated);
    }

    [Fact]
    public async Task Grant_UnknownSecret_ReportsSecretNotFound()
    {
        await using var provider = await fixture.CreatePandoraServiceProviderAsync();
        using var scope = provider.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IPermissionRepository>();

        var result = await repository.Grant(
            Random.Shared.NextInt64(1, long.MaxValue),
            NewItemId(),
            Guid.NewGuid(),
            CancellationToken.None);

        Assert.Equal(SecretAccessGrantResult.SecretNotFound, result);
    }

    [Fact]
    public async Task Revoke_ActivePermission_DeactivatesIt()
    {
        await using var provider = await fixture.CreatePandoraServiceProviderAsync();
        using var scope = provider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<PandoraDbContext>();
        var repository = scope.ServiceProvider.GetRequiredService<IPermissionRepository>();

        var itemId = NewItemId();
        var telegramUserId = Random.Shared.NextInt64(1, long.MaxValue);

        await AddSecret(context, itemId, SecretSyncOperation.None);
        await repository.Grant(telegramUserId, itemId, Guid.NewGuid(), CancellationToken.None);
        await repository.SaveChanges(CancellationToken.None);

        var result = await repository.Revoke(telegramUserId, itemId, CancellationToken.None);
        await repository.SaveChanges(CancellationToken.None);

        Assert.Equal(SecretAccessRevokeResult.Revoked, result);
        Assert.False(await repository.HasAccess(telegramUserId, itemId, CancellationToken.None));
    }

    [Fact]
    public async Task Revoke_RepeatedCall_ReportsAlreadyRevoked()
    {
        await using var provider = await fixture.CreatePandoraServiceProviderAsync();
        using var scope = provider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<PandoraDbContext>();
        var repository = scope.ServiceProvider.GetRequiredService<IPermissionRepository>();

        var itemId = NewItemId();
        var telegramUserId = Random.Shared.NextInt64(1, long.MaxValue);

        await AddSecret(context, itemId, SecretSyncOperation.None);
        await repository.Grant(telegramUserId, itemId, Guid.NewGuid(), CancellationToken.None);
        await repository.SaveChanges(CancellationToken.None);
        await repository.Revoke(telegramUserId, itemId, CancellationToken.None);
        await repository.SaveChanges(CancellationToken.None);

        var repeated = await repository.Revoke(telegramUserId, itemId, CancellationToken.None);

        Assert.Equal(SecretAccessRevokeResult.AlreadyRevoked, repeated);
    }

    private static string NewItemId()
    {
        return Guid.NewGuid().ToString("D");
    }

    private static async Task AddSecret(PandoraDbContext context, string itemId, SecretSyncOperation operation)
    {
        var metadata = new SecretMetadata(itemId, itemId, "integration-login");

        if (operation is not SecretSyncOperation.None)
        {
            metadata.MarkPending(operation);
        }

        context.SecretMetadata.Add(metadata);

        await context.SaveChangesAsync();
    }

    private static IOrderedQueryable<SecretMetadata> OrderByName(IQueryable<SecretMetadata> query)
    {
        return query.OrderBy(metadata => metadata.ResourceName).ThenBy(metadata => metadata.VaultItemId);
    }
}
