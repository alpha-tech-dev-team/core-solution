using System.Security.Claims;
using System.Text;
using CoreSolution.Shared.Options;
using Identity.Core.Constants;
using JwtConstants = Identity.Core.Constants.JwtConstants;
using Identity.Core.Entities;
using Identity.Core.Interfaces.Security;
using Identity.Core.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace Identity.Infrastructure.Services;

public sealed class JwtTokenService : IJwtTokenService
{
    private static readonly JsonWebTokenHandler TokenHandler = new() { MapInboundClaims = JwtConstants.MapInboundClaims };

    private readonly JwtOptions _options;
    private readonly TimeProvider _timeProvider;
    private readonly ILogger<JwtTokenService> _logger;
    private readonly SymmetricSecurityKey _signingKey;

    public JwtTokenService(IOptions<JwtOptions> options, TimeProvider timeProvider, ILogger<JwtTokenService> logger)
    {
        _options = options.Value;
        _timeProvider = timeProvider;
        _logger = logger;

        var secret = _options.Secret;
        ArgumentException.ThrowIfNullOrWhiteSpace(secret);
        ArgumentOutOfRangeException.ThrowIfLessThan(
            Encoding.UTF8.GetByteCount(secret),
            JwtOptions.MinimumSecretLength);

        _signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
    }

    public AuthTokens IssueTokens(User user)
    {
        var issuedAt = _timeProvider.GetUtcNow();
        var accessTokenExpiresAt = issuedAt.AddMinutes(_options.AccessTokenLifetimeMinutes);
        var refreshTokenExpiresAt = issuedAt.AddDays(_options.RefreshTokenLifetimeDays);

        return new AuthTokens(
            CreateToken(user, issuedAt, accessTokenExpiresAt, _options.Audience),
            accessTokenExpiresAt,
            CreateToken(user, issuedAt, refreshTokenExpiresAt, GetRefreshAudience()),
            refreshTokenExpiresAt);
    }

    public async Task<Guid?> GetUserIdFromRefreshToken(string refreshToken)
    {
        try
        {
            var validation = await TokenHandler.ValidateTokenAsync(refreshToken, CreateRefreshValidationParameters());

            if (!validation.IsValid)
            {
                _logger.LogInformation("Refresh-токен отклонен");
                return null;
            }

            return Guid.TryParse(validation.ClaimsIdentity.FindFirst(IdentityClaims.Subject)?.Value, out var userId)
                ? userId
                : null;
        }
        catch (Exception exception) when (exception is SecurityTokenException or ArgumentException)
        {
            _logger.LogInformation("Refresh-токен отклонен");
            return null;
        }
    }

    private string CreateToken(User user, DateTimeOffset issuedAt, DateTimeOffset expiresAt, string audience)
    {
        var descriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(BuildClaims(user)),
            Issuer = _options.Issuer,
            Audience = audience,
            IssuedAt = issuedAt.UtcDateTime,
            NotBefore = issuedAt.UtcDateTime,
            Expires = expiresAt.UtcDateTime,
            SigningCredentials = new SigningCredentials(_signingKey, JwtConstants.SigningAlgorithm)
        };

        return TokenHandler.CreateToken(descriptor);
    }

    private static IEnumerable<Claim> BuildClaims(User user)
    {
        yield return new Claim(IdentityClaims.Subject, user.Id.ToString());

        if (user.AdminUsername is not null)
        {
            yield return new Claim(IdentityClaims.UniqueName, user.AdminUsername);
        }

        foreach (var role in user.UserRoles
                     .Select(x => x.Role.Name)
                     .Distinct(StringComparer.OrdinalIgnoreCase))
        {
            yield return new Claim(IdentityClaims.Role, role);
        }
    }

    private TokenValidationParameters CreateRefreshValidationParameters()
    {
        return new TokenValidationParameters
        {
            ValidateIssuer = JwtConstants.ValidateIssuer,
            ValidIssuer = _options.Issuer,
            ValidateAudience = JwtConstants.ValidateAudience,
            ValidAudience = GetRefreshAudience(),
            ValidateIssuerSigningKey = JwtConstants.ValidateIssuerSigningKey,
            IssuerSigningKey = _signingKey,
            ValidateLifetime = JwtConstants.ValidateLifetime,
            ClockSkew = JwtConstants.ClockSkew,
            ValidAlgorithms = JwtConstants.ValidAlgorithms
        };
    }

    private string GetRefreshAudience()
    {
        return $"{_options.Audience}{JwtConstants.RefreshAudienceSuffix}";
    }
}
