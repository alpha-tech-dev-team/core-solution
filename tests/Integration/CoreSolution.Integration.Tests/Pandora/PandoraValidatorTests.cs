using FluentValidation;
using Pandora.Application.Features.Permissions;
using Pandora.Application.Features.Secrets.GetSecrets;
using Pandora.Application.Validators;
using Pandora.Domain.Constants;
using Xunit;

namespace CoreSolution.Integration.Tests.PandoraTests;

public sealed class PandoraValidatorTests
{
    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void GrantValidator_RejectsNonPositiveTelegramUserId(long telegramUserId)
    {
        var validator = new GrantSecretAccessMessageValidator();
        var message = new GrantSecretAccessMessage { TelegramUserId = telegramUserId, ItemId = "item-1" };

        Assert.False(validator.Validate(message).IsValid);
    }

    [Fact]
    public void GrantValidator_RejectsEmptyItemId()
    {
        var validator = new GrantSecretAccessMessageValidator();
        var message = new GrantSecretAccessMessage { TelegramUserId = 1, ItemId = string.Empty };

        Assert.False(validator.Validate(message).IsValid);
    }

    [Fact]
    public void GrantValidator_RejectsTooLongItemId()
    {
        var validator = new GrantSecretAccessMessageValidator();
        var message = new GrantSecretAccessMessage
        {
            TelegramUserId = 1,
            ItemId = new string('a', SecretMetadataLimits.VaultItemIdMaxLength + 1)
        };

        Assert.False(validator.Validate(message).IsValid);
    }

    [Fact]
    public void GrantValidator_AcceptsValidMessage()
    {
        var validator = new GrantSecretAccessMessageValidator();
        var message = new GrantSecretAccessMessage { TelegramUserId = 1, ItemId = "item-1" };

        Assert.True(validator.Validate(message).IsValid);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void RevokeValidator_RejectsNonPositiveTelegramUserId(long telegramUserId)
    {
        var validator = new RevokeSecretAccessMessageValidator();
        var message = new RevokeSecretAccessMessage { TelegramUserId = telegramUserId, ItemId = "item-1" };

        Assert.False(validator.Validate(message).IsValid);
    }

    [Fact]
    public void RevokeValidator_RejectsEmptyItemId()
    {
        var validator = new RevokeSecretAccessMessageValidator();
        var message = new RevokeSecretAccessMessage { TelegramUserId = 1, ItemId = string.Empty };

        Assert.False(validator.Validate(message).IsValid);
    }

    [Fact]
    public void GetSecretsValidator_RejectsMissingPagination()
    {
        var validator = new GetSecretsMessageValidator();

        Assert.False(validator.Validate(new GetSecretsMessage()).IsValid);
    }

    [Fact]
    public void GetSecretsValidator_RejectsPageBelowFirst()
    {
        var validator = new GetSecretsMessageValidator();
        var message = new GetSecretsMessage { Page = 0, PageSize = 20 };

        Assert.False(validator.Validate(message).IsValid);
    }

    [Fact]
    public void GetSecretsValidator_RejectsPageSizeAboveMaximum()
    {
        var validator = new GetSecretsMessageValidator();
        var message = new GetSecretsMessage { Page = 1, PageSize = 101 };

        Assert.False(validator.Validate(message).IsValid);
    }

    [Fact]
    public void GetSecretsValidator_AcceptsValidPagination()
    {
        var validator = new GetSecretsMessageValidator();
        var message = new GetSecretsMessage { Page = 1, PageSize = 20 };

        Assert.True(validator.Validate(message).IsValid);
    }

    [Fact]
    public void EmployeeSecretsValidator_RejectsMissingPagination()
    {
        var validator = new GetEmployeeSecretsMessageValidator();

        Assert.False(validator.Validate(new GetEmployeeSecretsMessage { TelegramUserId = 1 }).IsValid);
    }

    [Fact]
    public void Validators_AreResolvableFromValidatorType()
    {
        Assert.IsAssignableFrom<IValidator<GrantSecretAccessMessage>>(new GrantSecretAccessMessageValidator());
        Assert.IsAssignableFrom<IValidator<RevokeSecretAccessMessage>>(new RevokeSecretAccessMessageValidator());
    }

    [Fact]
    public void GetSecretsMessage_DefaultsToFirstPageAndDefaultPageSize()
    {
        var message = new GetSecretsMessage { Page = null, PageSize = null };

        Assert.Equal(1, message.EffectivePage);
        Assert.Equal(20, message.EffectivePageSize);
        Assert.Equal(0, message.Skip);
    }

    [Fact]
    public void GetSecretsMessage_ComputesSkipFromPage()
    {
        var message = new GetSecretsMessage { Page = 3, PageSize = 10 };

        Assert.Equal(20, message.Skip);
    }

    [Fact]
    public void GetSecretsMessage_ClampsNonPositivePageToFirst()
    {
        var message = new GetSecretsMessage { Page = -5, PageSize = 10 };

        Assert.Equal(1, message.EffectivePage);
        Assert.Equal(0, message.Skip);
    }

    [Fact]
    public void GetSecretsMessage_ClampsPageSizeToAllowedRange()
    {
        var tooBig = new GetSecretsMessage { Page = 1, PageSize = 500 };
        var tooSmall = new GetSecretsMessage { Page = 1, PageSize = 0 };

        Assert.Equal(100, tooBig.EffectivePageSize);
        Assert.Equal(1, tooSmall.EffectivePageSize);
    }
}
