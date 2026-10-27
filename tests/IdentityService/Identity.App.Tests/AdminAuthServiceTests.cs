using CoreSolution.Shared.Options;
using Identity.App.Errors;
using Identity.App.Services;
using Identity.Core.Entities;
using Identity.Core.Options;
using Identity.Infrastructure.Services;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace Identity.App.Tests;

public class AdminAuthServiceTests
{
    private static readonly JwtOptions Jwt = new()
    {
        Secret = "0123456789abcdef0123456789abcdef",
        Issuer = "test-issuer",
        Audience = "test-audience",
        AccessTokenLifetimeMinutes = 15,
        RefreshTokenLifetimeDays = 7
    };

    [Fact]
    public async Task Login_ReturnsTokens_ForValidCredentials()
    {
        var user = AdminUser("password");
        var service = CreateService(user);

        var result = await service.Login(user.AdminUsername!, "password");

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.False(string.IsNullOrWhiteSpace(result.Value.AccessToken));
        Assert.False(string.IsNullOrWhiteSpace(result.Value.RefreshToken));
    }

    [Fact]
    public async Task Login_ReturnsInvalidCredentials_ForWrongPassword()
    {
        var user = AdminUser("password");
        var service = CreateService(user);

        var result = await service.Login(user.AdminUsername!, "wrong-password");

        Assert.False(result.IsSuccess);
        Assert.Equal(nameof(IdentityErrorCode.InvalidCredentials), result.ErrorCode);
    }

    [Fact]
    public async Task Login_ReturnsInvalidCredentials_ForUnknownUsername()
    {
        var service = CreateService(null);

        var result = await service.Login("unknown", "password");

        Assert.False(result.IsSuccess);
        Assert.Equal(nameof(IdentityErrorCode.InvalidCredentials), result.ErrorCode);
    }

    [Fact]
    public async Task Refresh_ReturnsTokens_ForTokenFromLogin()
    {
        var user = AdminUser("password");
        var service = CreateService(user);
        var login = await service.Login(user.AdminUsername!, "password");

        var result = await service.Refresh(login.Value!.RefreshToken);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.False(string.IsNullOrWhiteSpace(result.Value.AccessToken));
    }

    [Fact]
    public async Task Refresh_ReturnsInvalidRefreshToken_ForAccessToken()
    {
        var user = AdminUser("password");
        var service = CreateService(user);
        var login = await service.Login(user.AdminUsername!, "password");

        var result = await service.Refresh(login.Value!.AccessToken);

        Assert.False(result.IsSuccess);
        Assert.Equal(nameof(IdentityErrorCode.InvalidRefreshToken), result.ErrorCode);
    }

    [Fact]
    public async Task Refresh_ReturnsInvalidRefreshToken_ForGarbage()
    {
        var user = AdminUser("password");
        var service = CreateService(user);

        var result = await service.Refresh("not-a-token");

        Assert.False(result.IsSuccess);
        Assert.Equal(nameof(IdentityErrorCode.InvalidRefreshToken), result.ErrorCode);
    }

    [Fact]
    public async Task Refresh_ReturnsInvalidRefreshToken_WhenUserWasDeleted()
    {
        var user = AdminUser("password");
        var login = await CreateService(user).Login(user.AdminUsername!, "password");

        var result = await CreateService(null).Refresh(login.Value!.RefreshToken);

        Assert.False(result.IsSuccess);
        Assert.Equal(nameof(IdentityErrorCode.InvalidRefreshToken), result.ErrorCode);
    }

    [Fact]
    public async Task Refresh_ReturnsInvalidRefreshToken_WhenPasswordHashWasRemoved()
    {
        var user = AdminUser("password");
        var service = CreateService(user);
        var login = await service.Login(user.AdminUsername!, "password");
        user.AdminPasswordHash = null;

        var result = await service.Refresh(login.Value!.RefreshToken);

        Assert.False(result.IsSuccess);
        Assert.Equal(nameof(IdentityErrorCode.InvalidRefreshToken), result.ErrorCode);
    }

    private static User AdminUser(string password)
    {
        return new User
        {
            Id = Guid.NewGuid(),
            Phone = "+375291234567",
            AdminUsername = "admin",
            AdminPasswordHash = CreatePasswordHasher().HashPassword(password)
        };
    }

    private static AdminAuthService CreateService(User? user)
    {
        return new AdminAuthService(
            new FakeUserRepository(user),
            CreatePasswordHasher(),
            new JwtTokenService(
                Options.Create(Jwt),
                TimeProvider.System,
                NullLogger<JwtTokenService>.Instance),
            NullLogger<AdminAuthService>.Instance);
    }

    private static PasswordHasher CreatePasswordHasher()
    {
        var options = Options.Create(new PasswordHasherOptions
        {
            MemorySize = 1024,
            Iterations = 1,
            Parallelism = 1
        });

        return new PasswordHasher(options, NullLogger<PasswordHasher>.Instance);
    }
}
