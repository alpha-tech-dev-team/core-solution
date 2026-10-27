using Identity.App.Errors;
using Identity.App.Services;
using Identity.Core.Entities;
using Identity.Core.Enums;
using Microsoft.Extensions.Logging.Abstractions;

namespace Identity.App.Tests;

public class IdentityServiceTests
{
    [Fact]
    public async Task ActivateUser_ActivatesPendingUser()
    {
        var user = new User { Id = Guid.NewGuid(), Phone = "+375291234567", Status = UserStatus.Pending };
        var service = CreateService(user);

        var result = await service.ActivateUser(42, user.Phone);

        Assert.True(result.IsSuccess);
        Assert.Equal(UserStatus.Active, user.Status);
        Assert.Equal(42, user.TelegramUserId);
    }

    [Fact]
    public async Task ActivateUser_ReturnsUserNotFound_WhenPhoneIsUnknown()
    {
        var service = CreateService(null);

        var result = await service.ActivateUser(42, "+375290000000");

        Assert.False(result.IsSuccess);
        Assert.Equal(nameof(IdentityErrorCode.UserNotFound), result.ErrorCode);
    }

    [Fact]
    public async Task ActivateUser_ReturnsUserAlreadyActivated_WhenPhoneBelongsToAnotherTelegramUser()
    {
        var user = new User { Id = Guid.NewGuid(), Phone = "+375291234567", TelegramUserId = 7, Status = UserStatus.Active };
        var service = CreateService(user);

        var result = await service.ActivateUser(42, user.Phone);

        Assert.False(result.IsSuccess);
        Assert.Equal(nameof(IdentityErrorCode.UserAlreadyActivated), result.ErrorCode);
        Assert.Equal(7, user.TelegramUserId);
    }

    [Fact]
    public async Task ActivateUser_Succeeds_WhenUserIsAlreadyActivatedForSameTelegramUser()
    {
        var user = new User { Id = Guid.NewGuid(), Phone = "+375291234567", TelegramUserId = 42, Status = UserStatus.Active };
        var service = CreateService(user);

        var result = await service.ActivateUser(42, user.Phone);

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task CheckUser_Succeeds_WhenActiveUserHasAccessRole()
    {
        var user = ActiveUser("Atlas");
        var service = CreateService(user);

        var result = await service.CheckUser(42, "atlas");

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task CheckUser_ReturnsAccessDenied_WhenUserIsPending()
    {
        var user = ActiveUser("Atlas");
        user.Status = UserStatus.Pending;
        var service = CreateService(user);

        var result = await service.CheckUser(42, "Atlas");

        Assert.False(result.IsSuccess);
        Assert.Equal(nameof(IdentityErrorCode.AccessDenied), result.ErrorCode);
    }

    [Fact]
    public async Task CheckUser_ReturnsAccessDenied_WhenUserHasNoMatchingRole()
    {
        var user = ActiveUser("Atlas");
        var service = CreateService(user);

        var result = await service.CheckUser(42, "Pandora");

        Assert.False(result.IsSuccess);
        Assert.Equal(nameof(IdentityErrorCode.AccessDenied), result.ErrorCode);
    }

    private static User ActiveUser(string role)
    {
        return new User
        {
            Id = Guid.NewGuid(),
            Phone = "+375291234567",
            TelegramUserId = 42,
            Status = UserStatus.Active,
            UserRoles =
            [
                new UserRole { Role = new Roles { Id = Guid.NewGuid(), Name = role } }
            ]
        };
    }

    private static IdentityService CreateService(User? user)
    {
        return new IdentityService(new FakeUserRepository(user), NullLogger<IdentityService>.Instance);
    }
}
