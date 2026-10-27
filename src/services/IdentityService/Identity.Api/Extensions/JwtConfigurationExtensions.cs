using System.Text;
using CoreSolution.Shared.Options;
using Identity.Api.Constants;
using Identity.Core.Constants;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

namespace Identity.Api.Extensions;

public static class JwtConfigurationExtensions
{
    public static IServiceCollection AddJwtAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        var section = configuration.GetSection(JwtOptions.SectionName);
        services.Configure<JwtOptions>(section);

        var options = section.Get<JwtOptions>() ?? new JwtOptions();
        var secret = options.Secret;

        ArgumentException.ThrowIfNullOrWhiteSpace(secret);
        ArgumentException.ThrowIfNullOrWhiteSpace(options.Issuer);
        ArgumentException.ThrowIfNullOrWhiteSpace(options.Audience);
        ArgumentOutOfRangeException.ThrowIfLessThan(
            Encoding.UTF8.GetByteCount(secret),
            JwtOptions.MinimumSecretLength);
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(options.AccessTokenLifetimeMinutes, 0);
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(options.RefreshTokenLifetimeDays, 0);

        services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(jwt =>
            {
                jwt.MapInboundClaims = JwtConstants.MapInboundClaims;
                jwt.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = JwtConstants.ValidateIssuer,
                    ValidIssuer = options.Issuer,
                    ValidateAudience = JwtConstants.ValidateAudience,
                    ValidAudience = options.Audience,
                    ValidateIssuerSigningKey = JwtConstants.ValidateIssuerSigningKey,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret)),
                    ValidateLifetime = JwtConstants.ValidateLifetime,
                    ClockSkew = JwtConstants.ClockSkew,
                    ValidAlgorithms = JwtConstants.ValidAlgorithms,
                    NameClaimType = IdentityClaims.UniqueName,
                    RoleClaimType = IdentityClaims.Role
                };
            });

        services.AddAuthorization(authorization =>
            authorization.AddPolicy(
                AuthorizationPolicies.Admin,
                policy => policy.RequireRole(AuthorizationPolicies.Admin)));

        return services;
    }
}
