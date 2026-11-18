using CoreSolution.Shared.Pagination;
using CoreSolution.Shared.Results;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Pandora.Application.Contracts;
using Pandora.Application.Features.Permissions;
using Pandora.Application.Features.Secrets.Common;
using Pandora.Application.Features.Secrets.GetSecrets;
using Pandora.Domain.Constants;
using Pandora.Domain.Entities;
using Pandora.Domain.Enums;
using Pandora.Domain.Interfaces;
using Pandora.Infrastructure.Database;
using Pandora.Infrastructure.Repositories;
using Xunit;

namespace CoreSolution.Integration.Tests.PandoraTests;

[Collection(nameof(IntegrationCollection))]
public sealed class EmployeeSecretsHandlerTests(IntegrationFixture fixture)
{
    [Fact]
    public async Task GetSecrets_WithoutPasswords_ReturnsPageWithoutPasswordValues()
    {
        await using var provider = await fixture.CreatePandoraServiceProviderAsync();
        using var scope = provider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<PandoraDbContext>();

        var itemId = await AddSecret(context, "integration-plain-secret");
        var vaultReader = new StubVaultReader((itemId, "stored-password"));
        var handler = CreateGetSecretsHandler(context, vaultReader, new CapturingAuditSaver(new AuditContext()));

        var result = await handler.Handle(
            new GetSecretsMessage
            {
                Page = PaginationLimits.FirstPage,
                PageSize = PaginationLimits.MaxPageSize,
                IncludePasswords = false
            },
            CancellationToken.None);

        var item = result.Value!.Items.Single(secret => secret.Id == itemId);

        Assert.Null(item.Password);
        Assert.Equal(0, vaultReader.CallCount);
    }

    [Fact]
    public async Task GetSecrets_WithPasswords_AttachesPasswordsAndWritesAuditPerSecret()
    {
        await using var provider = await fixture.CreatePandoraServiceProviderAsync();
        using var scope = provider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<PandoraDbContext>();

        var itemId = await AddSecret(context, "integration-audited-secret");
        var adminUserId = Guid.NewGuid();
        var vaultReader = new StubVaultReader((itemId, "revealed-password"));
        var auditContext = new AuditContext();
        var auditSaver = new CapturingAuditSaver(auditContext);
        var handler = CreateGetSecretsHandler(context, vaultReader, auditSaver);

        var result = await handler.Handle(
            WithAdmin(new GetSecretsMessage
            {
                Page = PaginationLimits.FirstPage,
                PageSize = PaginationLimits.MaxPageSize,
                IncludePasswords = true
            }, adminUserId),
            CancellationToken.None);

        var item = result.Value!.Items.Single(secret => secret.Id == itemId);

        Assert.Equal("revealed-password", item.Password);
        Assert.Equal(1, vaultReader.CallCount);
        Assert.Equal(1, auditSaver.SaveCount);

        var entry = Assert.Single(
            auditSaver.Entries,
            candidate => candidate.TargetItemId == itemId);

        Assert.Equal(AuditConstants.ActionSecretPasswordsViewed, entry.Action);
        Assert.Equal(adminUserId, entry.UserId);
        Assert.Equal(auditSaver.Entries.Count, auditSaver.Entries.Select(candidate => candidate.TargetItemId).Distinct().Count());

        foreach (var auditedEntry in auditSaver.Entries)
        {
            Assert.True(auditedEntry.TargetItemId!.Length <= DatabaseConstants.Audit.TargetItemIdMaxLength);
        }
    }

    [Fact]
    public async Task GetSecrets_VaultItemsWithDuplicateIds_DoesNotFail()
    {
        await using var provider = await fixture.CreatePandoraServiceProviderAsync();
        using var scope = provider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<PandoraDbContext>();

        var itemId = await AddSecret(context, "integration-duplicate-secret");
        var vaultReader = new StubVaultReader((itemId, "first-password"), (itemId, "second-password"));
        var handler = CreateGetSecretsHandler(context, vaultReader, new CapturingAuditSaver(new AuditContext()));

        var result = await handler.Handle(
            new GetSecretsMessage
            {
                Page = PaginationLimits.FirstPage,
                PageSize = PaginationLimits.MaxPageSize,
                IncludePasswords = true
            },
            CancellationToken.None);

        var item = result.Value!.Items.Single(secret => secret.Id == itemId);

        Assert.Equal("first-password", item.Password);
    }

    [Fact]
    public async Task GetSecrets_WithPasswords_AuditsEverySecretOnPage()
    {
        await using var provider = await fixture.CreatePandoraServiceProviderAsync();
        using var scope = provider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<PandoraDbContext>();

        var revealedId = await AddSecret(context, "integration-revealed-secret");
        var missingId = await AddSecret(context, "integration-not-in-vault-secret");
        var vaultReader = new StubVaultReader((revealedId, "revealed-password"));
        var auditSaver = new CapturingAuditSaver(new AuditContext());
        var handler = CreateGetSecretsHandler(context, vaultReader, auditSaver);

        var result = await handler.Handle(
            WithAdmin(
                new GetSecretsMessage
                {
                    Page = PaginationLimits.FirstPage,
                    PageSize = PaginationLimits.MaxPageSize,
                    IncludePasswords = true
                },
                Guid.NewGuid()),
            CancellationToken.None);

        Assert.Equal("revealed-password", result.Value!.Items.Single(secret => secret.Id == revealedId).Password);
        Assert.Null(result.Value.Items.Single(secret => secret.Id == missingId).Password);

        var revealedEntry = Assert.Single(auditSaver.Entries, entry => entry.TargetItemId == revealedId);
        var missingEntry = Assert.Single(auditSaver.Entries, entry => entry.TargetItemId == missingId);

        Assert.Equal(AuditConstants.ActionSecretPasswordsViewed, revealedEntry.Action);
        Assert.Equal(AuditConstants.ActionSecretListViewed, missingEntry.Action);

        Assert.Equal(
            result.Value.Items.Count,
            auditSaver.Entries.Select(entry => entry.TargetItemId).Distinct().Count());
    }

    [Fact]
    public async Task GetSecrets_SecondPage_ReturnsDifferentSecrets_AndTotalStaysStable()
    {
        await using var provider = await fixture.CreatePandoraServiceProviderAsync();
        using var scope = provider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<PandoraDbContext>();

        await AddSecret(context, "integration-page-secret");
        await AddSecret(context, "integration-page-secret-2");
        var handler = CreateGetSecretsHandler(context, new StubVaultReader(), new CapturingAuditSaver(new AuditContext()));

        var first = await handler.Handle(
            new GetSecretsMessage { Page = PaginationLimits.FirstPage, PageSize = 1 },
            CancellationToken.None);
        var second = await handler.Handle(
            new GetSecretsMessage { Page = PaginationLimits.FirstPage + 1, PageSize = 1 },
            CancellationToken.None);

        Assert.Single(first.Value!.Items);
        Assert.Single(second.Value!.Items);
        Assert.Equal(first.Value.Total, second.Value.Total);
        Assert.NotEqual(first.Value.Items[0].Id, second.Value.Items[0].Id);
    }

    [Fact]
    public async Task GetEmployeeSecrets_MarksOnlyGrantedSecrets()
    {
        await using var provider = await fixture.CreatePandoraServiceProviderAsync();
        using var scope = provider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<PandoraDbContext>();

        var grantedId = await AddSecret(context, "integration-granted-secret");
        await AddSecret(context, "integration-not-granted-secret");

        var telegramUserId = Random.Shared.NextInt64(1, long.MaxValue);
        var repository = new PermissionRepository(context);
        await repository.Grant(telegramUserId, grantedId, Guid.NewGuid(), CancellationToken.None);
        await repository.SaveChanges(CancellationToken.None);

        var handler = new GetEmployeeSecretsHandler(repository);

        var result = await handler.Handle(
            new GetEmployeeSecretsMessage
            {
                TelegramUserId = telegramUserId,
                Page = PaginationLimits.FirstPage,
                PageSize = PaginationLimits.MaxPageSize
            },
            CancellationToken.None);

        var granted = result.Value!.Items.Single(item => item.Id == grantedId);

        Assert.True(granted.IsGranted);
        Assert.Contains(result.Value.Items, item => !item.IsGranted);
    }

    [Fact]
    public async Task GetEmployeeSecrets_ExcludesSecretsMarkedForDeletion()
    {
        await using var provider = await fixture.CreatePandoraServiceProviderAsync();
        using var scope = provider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<PandoraDbContext>();

        var deletedId = await AddSecret(context, "integration-deleting-secret", SecretSyncOperation.Delete);
        var repository = new PermissionRepository(context);
        var handler = new GetEmployeeSecretsHandler(repository);

        var result = await handler.Handle(
            new GetEmployeeSecretsMessage
            {
                TelegramUserId = Random.Shared.NextInt64(1, long.MaxValue),
                Page = PaginationLimits.FirstPage,
                PageSize = PaginationLimits.MaxPageSize
            },
            CancellationToken.None);

        Assert.DoesNotContain(result.Value!.Items, item => item.Id == deletedId);
    }

    [Fact]
    public async Task Grant_StoresAuditWithTargetEmployee()
    {
        await using var provider = await fixture.CreatePandoraServiceProviderAsync();
        using var scope = provider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<PandoraDbContext>();

        var itemId = await AddSecret(context, "integration-grant-audit");
        var telegramUserId = Random.Shared.NextInt64(1, long.MaxValue);
        var adminUserId = Guid.NewGuid();
        var auditContext = new AuditContext();
        var handler = new GrantSecretAccessHandler(
            new PermissionRepository(context),
            new SecretAuditLogger(auditContext),
            NullLogger<GrantSecretAccessHandler>.Instance);

        var result = await handler.Handle(
            WithAdmin(
                new GrantSecretAccessMessage
                {
                    TelegramUserId = telegramUserId,
                    ItemId = itemId
                },
                adminUserId),
            CancellationToken.None);

        Assert.True(result.IsSuccess);

        var entry = Assert.Single(auditContext.TakeAll());

        Assert.Equal(AuditConstants.ActionSecretAccessGranted, entry.Action);
        Assert.Equal(itemId, entry.TargetItemId);
        Assert.Equal(adminUserId, entry.UserId);
        Assert.Equal(telegramUserId, entry.TargetTelegramUserId);
    }

    [Fact]
    public async Task Grant_UnknownSecret_DoesNotWriteAudit()
    {
        await using var provider = await fixture.CreatePandoraServiceProviderAsync();
        using var scope = provider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<PandoraDbContext>();

        var auditContext = new AuditContext();
        var handler = new GrantSecretAccessHandler(
            new PermissionRepository(context),
            new SecretAuditLogger(auditContext),
            NullLogger<GrantSecretAccessHandler>.Instance);

        var result = await handler.Handle(
            WithAdmin(
                new GrantSecretAccessMessage
                {
                    TelegramUserId = Random.Shared.NextInt64(1, long.MaxValue),
                    ItemId = Guid.NewGuid().ToString("D")
                },
                Guid.NewGuid()),
            CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Empty(auditContext.TakeAll());
    }

    [Fact]
    public async Task Revoke_StoresAuditWithTargetEmployee()
    {
        await using var provider = await fixture.CreatePandoraServiceProviderAsync();
        using var scope = provider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<PandoraDbContext>();

        var itemId = await AddSecret(context, "integration-revoke-audit");
        var telegramUserId = Random.Shared.NextInt64(1, long.MaxValue);
        var auditContext = new AuditContext();
        var repository = new PermissionRepository(context);

        await repository.Grant(telegramUserId, itemId, Guid.NewGuid(), CancellationToken.None);
        await repository.SaveChanges(CancellationToken.None);

        var handler = new RevokeSecretAccessHandler(
            repository,
            new SecretAuditLogger(auditContext),
            NullLogger<RevokeSecretAccessHandler>.Instance);

        var result = await handler.Handle(
            WithAdmin(
                new RevokeSecretAccessMessage
                {
                    TelegramUserId = telegramUserId,
                    ItemId = itemId
                },
                Guid.NewGuid()),
            CancellationToken.None);

        Assert.True(result.IsSuccess);

        var entry = Assert.Single(auditContext.TakeAll());

        Assert.Equal(AuditConstants.ActionSecretAccessRevoked, entry.Action);
        Assert.Equal(telegramUserId, entry.TargetTelegramUserId);
    }

    [Fact]
    public async Task GetSecrets_VaultUnavailable_ReturnsVaultwardenUnavailable()
    {
        await using var provider = await fixture.CreatePandoraServiceProviderAsync();
        using var scope = provider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<PandoraDbContext>();

        await AddSecret(context, "integration-vault-down-secret");
        var auditSaver = new CapturingAuditSaver(new AuditContext());
        var handler = CreateGetSecretsHandler(
            context,
            new FailingVaultReader(),
            auditSaver);

        var result = await handler.Handle(
            new GetSecretsMessage
            {
                Page = PaginationLimits.FirstPage,
                PageSize = PaginationLimits.MaxPageSize,
                IncludePasswords = true
            },
            CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(
            Pandora.Domain.Enums.ErrorCode.VaultwardenUnavailable.ToString(),
            result.ErrorCode);

        Assert.Equal(0, auditSaver.SaveCount);
        Assert.Empty(auditSaver.Entries);
    }

    [Fact]
    public async Task GetSecrets_RequestedOnlyPagePasswords()
    {
        await using var provider = await fixture.CreatePandoraServiceProviderAsync();
        using var scope = provider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<PandoraDbContext>();

        var firstId = await AddSecret(context, "integration-page-a");
        var secondId = await AddSecret(context, "integration-page-b");
        var vaultReader = new StubVaultReader(
            (firstId, "first-password"),
            (secondId, "second-password"));
        var handler = CreateGetSecretsHandler(
            context,
            vaultReader,
            new CapturingAuditSaver(new AuditContext()));

        var result = await handler.Handle(
            new GetSecretsMessage
            {
                Page = PaginationLimits.FirstPage,
                PageSize = PaginationLimits.MaxPageSize,
                IncludePasswords = true
            },
            CancellationToken.None);

        Assert.True(result.IsSuccess);

        var pageIds = result.Value!.Items.Select(item => item.Id).ToList();

        Assert.NotNull(vaultReader.RequestedIds);
        Assert.Equal(pageIds.Count, vaultReader.RequestedIds!.Count);
        Assert.All(vaultReader.RequestedIds, id => Assert.Contains(id, pageIds));
    }

    private static T WithAdmin<T>(T message, Guid adminUserId)
        where T : IAdminUserRequest
    {
        message.AdminUserId = adminUserId;

        return message;
    }

    private static GetSecretsHandler CreateGetSecretsHandler(
        PandoraDbContext context,
        ISecretVaultReader vaultReader,
        CapturingAuditSaver auditSaver)
    {
        return new GetSecretsHandler(
            new SecretMetadataRepository(context),
            vaultReader,
            new SecretAuditLogger(auditSaver.AuditContext),
            auditSaver,
            NullLogger<GetSecretsHandler>.Instance);
    }

    private static async Task<string> AddSecret(
        PandoraDbContext context,
        string resourceName,
        SecretSyncOperation operation = SecretSyncOperation.None)
    {
        var itemId = Guid.NewGuid().ToString("D");
        var metadata = new SecretMetadata(itemId, resourceName, "integration-login");

        if (operation is not SecretSyncOperation.None)
        {
            metadata.MarkPending(operation);
        }

        context.SecretMetadata.Add(metadata);

        await context.SaveChangesAsync();

        return itemId;
    }

    private sealed class StubVaultReader : ISecretVaultReader
    {
        private readonly IReadOnlyList<(string Id, string Password)> _items;

        public StubVaultReader(params (string Id, string Password)[] items)
        {
            _items = items;
        }

        public int CallCount { get; private set; }

        public IReadOnlyCollection<string>? RequestedIds { get; private set; }

        public Task<Result<IReadOnlyDictionary<string, string>>> GetPasswords(
            IReadOnlyCollection<string> itemIds,
            CancellationToken cancellationToken)
        {
            CallCount += 1;
            RequestedIds = itemIds;

            var passwords = new Dictionary<string, string>();

            foreach (var item in _items)
            {
                if (itemIds.Contains(item.Id) && !passwords.ContainsKey(item.Id))
                {
                    passwords[item.Id] = item.Password;
                }
            }

            return Task.FromResult(Result<IReadOnlyDictionary<string, string>>.Success(passwords));
        }
    }

    private sealed class FailingVaultReader : ISecretVaultReader
    {
        public Task<Result<IReadOnlyDictionary<string, string>>> GetPasswords(
            IReadOnlyCollection<string> itemIds,
            CancellationToken cancellationToken)
        {
            return Task.FromResult(
                Result<IReadOnlyDictionary<string, string>>.Failure(
                    Pandora.Domain.Enums.ErrorCode.VaultwardenUnavailable.ToString()));
        }
    }

    private sealed class CapturingAuditSaver : IAuditSaver
    {
        public CapturingAuditSaver(IAuditContext auditContext)
        {
            AuditContext = auditContext;
        }

        public IAuditContext AuditContext { get; }
        public int SaveCount { get; private set; }
        public IReadOnlyList<AuditEntry> Entries { get; private set; } = Array.Empty<AuditEntry>();

        public Task SaveChanges(CancellationToken cancellationToken)
        {
            SaveCount += 1;
            Entries = AuditContext.TakeAll();

            return Task.CompletedTask;
        }
    }
}
