using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Pandora.Application.Features.Secrets.Common;
using Pandora.Application.Features.Settings.CredentialAccess;
using Pandora.Application.Validators;
using Pandora.Domain.Constants;
using Pandora.Domain.Entities;
using Pandora.Domain.Interfaces;
using Pandora.Domain.Options;
using Pandora.Infrastructure.Database;
using Xunit;

namespace CoreSolution.Integration.Tests.PandoraTests;

[Collection(nameof(IntegrationCollection))]
public sealed class CredentialAccessSettingsTests(IntegrationFixture fixture)
{
    [Fact]
    public async Task GetSettings_WithoutStoredRow_ReturnsDefaultTtl()
    {
        await using var provider = await fixture.CreatePandoraServiceProviderAsync();

        await ResetSettingsAsync(provider);

        using var scope = provider.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<ICredentialAccessSettingsRepository>();

        var settings = await repository.Get(CancellationToken.None);

        Assert.Equal(CredentialAccessConstants.DefaultTtlMinutes, settings.TtlMinutes);
        Assert.Null(settings.UpdatedAt);
    }

    [Fact]
    public async Task UpdateSettings_PersistsSelectedTtl()
    {
        await using var provider = await fixture.CreatePandoraServiceProviderAsync();

        await ResetSettingsAsync(provider);

        var adminUserId = Guid.NewGuid();

        using (var scope = provider.CreateScope())
        {
            var repository = scope.ServiceProvider.GetRequiredService<ICredentialAccessSettingsRepository>();

            var updated = await repository.Update(5, adminUserId, CancellationToken.None);

            Assert.Equal(5, updated.TtlMinutes);
            Assert.NotNull(updated.UpdatedAt);
        }

        using (var scope = provider.CreateScope())
        {
            var repository = scope.ServiceProvider.GetRequiredService<ICredentialAccessSettingsRepository>();

            var settings = await repository.Get(CancellationToken.None);

            Assert.Equal(5, settings.TtlMinutes);
            Assert.NotNull(settings.UpdatedAt);
        }
    }

    [Theory]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(5)]
    public async Task Validator_AllowsSupportedTtlValues(int ttlMinutes)
    {
        var validator = new UpdateCredentialAccessSettingsMessageValidator();

        var validation = await validator.ValidateAsync(
            new UpdateCredentialAccessSettingsMessage
            {
                TtlMinutes = ttlMinutes
            });

        Assert.True(validation.IsValid);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(4)]
    [InlineData(10)]
    public async Task Validator_RejectsUnsupportedTtlValues(int ttlMinutes)
    {
        var validator = new UpdateCredentialAccessSettingsMessageValidator();

        var validation = await validator.ValidateAsync(
            new UpdateCredentialAccessSettingsMessage
            {
                TtlMinutes = ttlMinutes
            });

        Assert.False(validation.IsValid);
    }

    [Fact]
    public async Task UpdateSettings_WritesAuditEntry()
    {
        await using var provider = await fixture.CreatePandoraServiceProviderAsync();

        await ResetSettingsAsync(provider);

        var adminUserId = new Guid("11111111-1111-1111-1111-111111111111");
        var message = new UpdateCredentialAccessSettingsMessage { TtlMinutes = 5 };
        var auditContext = new CapturingAuditContext();

        using (var scope = provider.CreateScope())
        {
            var handler = new UpdateCredentialAccessSettingsHandler(
                scope.ServiceProvider.GetRequiredService<ICredentialAccessSettingsRepository>(),
                new SecretAuditLogger(auditContext));

            var result = await handler.Handle(
                WithAdmin(message, adminUserId),
                CancellationToken.None);

            Assert.True(result.IsSuccess);
        }

        var auditEntries = auditContext.TakeAll();

        using (var verifyScope = provider.CreateScope())
        {
            var context = verifyScope.ServiceProvider.GetRequiredService<PandoraDbContext>();

            context.AddRange(auditEntries);
            await context.SaveChangesAsync();

            var entry = await context.AuditEntry
                .AsNoTracking()
                .OrderByDescending(item => item.Timestamp)
                .FirstOrDefaultAsync(item => item.Action == AuditConstants.ActionCredentialAccessSettingsUpdated);

            Assert.NotNull(entry);
            Assert.Equal(adminUserId, entry.UserId);
            Assert.Equal(AuditConstants.SourceWeb, entry.RequestSource);
        }
    }

    [Theory]
    [InlineData(nameof(CredentialCleanupOptions.PollIntervalSeconds))]
    [InlineData(nameof(CredentialCleanupOptions.BatchSize))]
    [InlineData(nameof(CredentialCleanupOptions.CompletedRetentionHours))]
    [InlineData(nameof(CredentialCleanupOptions.DeleteAttemptsBeforeFallback))]
    [InlineData(nameof(CredentialCleanupOptions.MaxAttempts))]
    [InlineData(nameof(CredentialCleanupOptions.RetryBaseDelaySeconds))]
    [InlineData(nameof(CredentialCleanupOptions.MaxRetryDelaySeconds))]
    public void CredentialCleanupOptions_WhenValueIsNotPositive_Throws(string propertyName)
    {
        var options = new CredentialCleanupOptions();
        typeof(CredentialCleanupOptions).GetProperty(propertyName)!.SetValue(options, 0);

        Assert.Throws<ArgumentOutOfRangeException>(options.Validate);
    }

    [Fact]
    public void CredentialCleanupOptions_WhenMaxRetryDelayBelowBaseDelay_Throws()
    {
        var options = new CredentialCleanupOptions
        {
            RetryBaseDelaySeconds = 30,
            MaxRetryDelaySeconds = 10
        };

        Assert.Throws<ArgumentException>(options.Validate);
    }

    [Fact]
    public void CredentialCleanupOptions_WhenDefaults_AreValid()
    {
        new CredentialCleanupOptions().Validate();
    }

    private static UpdateCredentialAccessSettingsMessage WithAdmin(
        UpdateCredentialAccessSettingsMessage message,
        Guid adminUserId)
    {
        ((Pandora.Application.Contracts.IAdminUserRequest)message).AdminUserId = adminUserId;

        return message;
    }

    private sealed class CapturingAuditContext : IAuditContext
    {
        private readonly List<AuditEntry> _entries = new();

        public void Enqueue(AuditEntry entry)
        {
            _entries.Add(entry);
        }

        public IReadOnlyList<AuditEntry> TakeAll()
        {
            var entries = _entries.ToArray();
            _entries.Clear();

            return entries;
        }
    }

    private static async Task ResetSettingsAsync(IServiceProvider provider)
    {
        using var scope = provider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<PandoraDbContext>();

        context.CredentialAccessSetting.RemoveRange(context.CredentialAccessSetting);

        await context.SaveChangesAsync();
    }
}
