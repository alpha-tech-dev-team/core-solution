using Identity.App.Services;
using Identity.Core.Entities;
using Identity.Core.Enums;
using Identity.Core.Interfaces.Repositories;
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

        Assert.True(result);
        Assert.Equal(UserStatus.Active, user.Status);
        Assert.Equal(42, user.TelegramUserId);
    }

    [Fact]
    public async Task ActivateUser_ReturnsFalse_WhenPhoneIsUnknown()
    {
        var service = CreateService(null);

        var result = await service.ActivateUser(42, "+375290000000");

        Assert.False(result);
    }

    [Fact]
    public async Task ActivateUser_ReturnsFalse_WhenPhoneBelongsToAnotherTelegramUser()
    {
        var user = new User { Id = Guid.NewGuid(), Phone = "+375291234567", TelegramUserId = 7, Status = UserStatus.Active };
        var service = CreateService(user);

        var result = await service.ActivateUser(42, user.Phone);

        Assert.False(result);
        Assert.Equal(7, user.TelegramUserId);
    }

    [Fact]
    public async Task CheckUser_ReturnsTrue_WhenActiveUserHasAccessRole()
    {
        var user = ActiveUser("Atlas");
        var service = CreateService(user);

        var result = await service.CheckUser(42, "atlas");

        Assert.True(result);
    }

    [Fact]
    public async Task CheckUser_ReturnsFalse_WhenUserIsPending()
    {
        var user = ActiveUser("Atlas");
        user.Status = UserStatus.Pending;
        var service = CreateService(user);

        var result = await service.CheckUser(42, "Atlas");

        Assert.False(result);
    }

    [Fact]
    public async Task CheckUser_ReturnsFalse_WhenUserHasNoMatchingRole()
    {
        var user = ActiveUser("Atlas");
        var service = CreateService(user);

        var result = await service.CheckUser(42, "Pandora");

        Assert.False(result);
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

    private class FakeUserRepository : IUserRepository
    {
        private readonly User? _user;

        public FakeUserRepository(User? user)
        {
            _user = user;
        }

        public Task<User?> GetByPhone(string phone, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(_user?.Phone == phone ? _user : null);
        }

        public Task<User?> GetByTelegramUserId(long telegramUserId, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(_user?.TelegramUserId == telegramUserId ? _user : null);
        }

        public Task SaveChanges(CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }
    }
}
