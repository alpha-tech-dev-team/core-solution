using Identity.App.Contracts;
using Identity.App.Validators;

namespace Identity.App.Tests;

public class ValidatorTests
{
    [Fact]
    public void ActivateUserRequest_IsValid_WithPhoneAndTelegramUserId()
    {
        var result = new ActivateUserRequestValidator().Validate(new ActivateUserRequest(42, "+375291234567"));

        Assert.True(result.IsValid);
    }

    [Fact]
    public void ActivateUserRequest_IsInvalid_WithoutPhoneAndTelegramUserId()
    {
        var result = new ActivateUserRequestValidator().Validate(new ActivateUserRequest(0, string.Empty));

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, x => x.PropertyName == nameof(ActivateUserRequest.Phone));
        Assert.Contains(result.Errors, x => x.PropertyName == nameof(ActivateUserRequest.TelegramUserId));
    }

    [Fact]
    public void CheckUserRequest_IsInvalid_WithoutAccess()
    {
        var result = new CheckUserRequestValidator().Validate(new CheckUserRequest(42, string.Empty));

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, x => x.PropertyName == nameof(CheckUserRequest.Access));
    }
}
