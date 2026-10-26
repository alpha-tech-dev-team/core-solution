using CoreSolution.Shared.Options;
using Identity.Core.Entities;
using Identity.Infrastructure.Services;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace Identity.Infrastructure.Tests;

public class JwtTokenServiceTests
{
    private const string Secret = "0123456789abcdef0123456789abcdef";
    private const string Issuer = "test-issuer";
    private const string Audience = "test-audience";

    private static readonly DateTimeOffset Now =
        DateTimeOffset.FromUnixTimeSeconds(DateTimeOffset.UtcNow.ToUnixTimeSeconds());

    [Fact]
    public void IssueTokens_FillsAccessTokenClaimsAndExpiry()
    {
        var user = AdminUser("Admin", "Atlas");
        var service = CreateService();

        var tokens = service.IssueTokens(user);
        var accessToken = new JsonWebToken(tokens.AccessToken);

        Assert.Equal(user.Id.ToString(), accessToken.Subject);
        Assert.Equal("admin", accessToken.GetClaim("unique_name").Value);
        Assert.Equal(["Admin", "Atlas"], accessToken.Claims.Where(x => x.Type == "role").Select(x => x.Value).Order());
        Assert.Equal(Audience, accessToken.Audiences.Single());
        Assert.Equal(Now.AddMinutes(15).UtcDateTime, accessToken.ValidTo);
        Assert.Equal(Now.AddMinutes(15), tokens.AccessTokenExpiresAt);
    }

    [Fact]
    public void IssueTokens_UsesLongerLifetimeForRefreshToken()
    {
        var tokens = CreateService().IssueTokens(AdminUser());
        var refreshToken = new JsonWebToken(tokens.RefreshToken);

        Assert.Equal($"{Audience}:refresh", refreshToken.Audiences.Single());
        Assert.Equal(Now.AddDays(7).UtcDateTime, refreshToken.ValidTo);
        Assert.Equal(Now.AddDays(7), tokens.RefreshTokenExpiresAt);
    }

    [Fact]
    public void IssueTokens_WritesRoleClaimOncePerRole()
    {
        var tokens = CreateService().IssueTokens(AdminUser("Admin", "admin", "Atlas"));

        var roles = new JsonWebToken(tokens.AccessToken).Claims
            .Where(x => x.Type == "role")
            .Select(x => x.Value);

        Assert.Equal(["Admin", "Atlas"], roles.Order());
    }

    [Fact]
    public async Task GetUserIdFromRefreshToken_ReturnsUserId_ForRefreshToken()
    {
        var user = AdminUser();
        var tokens = CreateService().IssueTokens(user);

        var userId = await CreateService().GetUserIdFromRefreshToken(tokens.RefreshToken);

        Assert.Equal(user.Id, userId);
    }

    [Fact]
    public async Task GetUserIdFromRefreshToken_ReturnsNull_ForAccessToken()
    {
        var tokens = CreateService().IssueTokens(AdminUser());

        var userId = await CreateService().GetUserIdFromRefreshToken(tokens.AccessToken);

        Assert.Null(userId);
    }

    [Fact]
    public async Task GetUserIdFromRefreshToken_ReturnsNull_ForGarbage()
    {
        Assert.Null(await CreateService().GetUserIdFromRefreshToken("not-a-token"));
    }

    [Fact]
    public async Task GetUserIdFromRefreshToken_ReturnsNull_ForTokenSignedWithAnotherSecret()
    {
        var tokens = CreateService().IssueTokens(AdminUser());
        var serviceWithOtherSecret = CreateService("fedcba9876543210fedcba9876543210");

        Assert.Null(await serviceWithOtherSecret.GetUserIdFromRefreshToken(tokens.RefreshToken));
    }

    [Fact]
    public async Task GetUserIdFromRefreshToken_ReturnsNull_ForExpiredRefreshToken()
    {
        var tokens = CreateService(now: Now.AddDays(-8)).IssueTokens(AdminUser());

        Assert.Null(await CreateService().GetUserIdFromRefreshToken(tokens.RefreshToken));
    }

    [Fact]
    public async Task GetUserIdFromRefreshToken_ReturnsNull_ForTokenWithAnotherIssuer()
    {
        var tokens = CreateService(issuer: "another-issuer").IssueTokens(AdminUser());

        Assert.Null(await CreateService().GetUserIdFromRefreshToken(tokens.RefreshToken));
    }

    [Fact]
    public async Task AccessToken_IsAcceptedByApiValidationParameters()
    {
        var tokens = CreateService().IssueTokens(AdminUser());

        Assert.True(await ValidateWithApiParameters(tokens.AccessToken));
    }

    [Fact]
    public void Constructor_Throws_WhenSecretIsTooShort()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => CreateService("short"));
    }

    private static async Task<bool> ValidateWithApiParameters(string token)
    {
        var validationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = Issuer,
            ValidateAudience = true,
            ValidAudience = Audience,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(Secret)),
            ValidateLifetime = false,
            ValidAlgorithms = [SecurityAlgorithms.HmacSha256]
        };

        var validation = await new JsonWebTokenHandler().ValidateTokenAsync(token, validationParameters);

        return validation.IsValid;
    }

    private static JwtTokenService CreateService(
        string secret = Secret,
        string issuer = Issuer,
        DateTimeOffset? now = null)
    {
        var options = Options.Create(new JwtOptions
        {
            Secret = secret,
            Issuer = issuer,
            Audience = Audience,
            AccessTokenLifetimeMinutes = 15,
            RefreshTokenLifetimeDays = 7
        });

        return new JwtTokenService(
            options,
            new FixedTimeProvider(now ?? Now),
            NullLogger<JwtTokenService>.Instance);
    }

    private static User AdminUser(params string[] roles)
    {
        return new User
        {
            Id = Guid.NewGuid(),
            Phone = "+375291234567",
            AdminUsername = "admin",
            UserRoles = roles
                .Select(role => new UserRole { Role = new Roles { Id = Guid.NewGuid(), Name = role } })
                .ToList()
        };
    }

    private sealed class FixedTimeProvider(DateTimeOffset utcNow) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow()
        {
            return utcNow;
        }
    }
}
