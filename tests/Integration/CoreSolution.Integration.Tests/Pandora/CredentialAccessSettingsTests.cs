using Microsoft.Extensions.DependencyInjection;
using Pandora.Application.Features.Settings.CredentialAccess;
using Pandora.Application.Validators;
using Pandora.Domain.Constants;
using Pandora.Domain.Interfaces;
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

    private static async Task ResetSettingsAsync(IServiceProvider provider)
    {
        using var scope = provider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<PandoraDbContext>();

        context.CredentialAccessSetting.RemoveRange(context.CredentialAccessSetting);

        await context.SaveChangesAsync();
    }
}
