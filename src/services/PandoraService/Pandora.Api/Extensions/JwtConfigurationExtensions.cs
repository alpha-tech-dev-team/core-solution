using System.Text;
using CoreSolution.Shared.Options;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Pandora.Api.Constants;

namespace Pandora.Api.Extensions;

public static class JwtConfigurationExtensions
{
    public static IServiceCollection AddJwtAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        var section = configuration.GetSection(JwtOptions.SectionName);
        var options = section.Get<JwtOptions>() ?? new JwtOptions();
        var secret = options.Secret;

        ArgumentException.ThrowIfNullOrWhiteSpace(secret);
        ArgumentException.ThrowIfNullOrWhiteSpace(options.Issuer);
        ArgumentException.ThrowIfNullOrWhiteSpace(options.Audience);
        ArgumentOutOfRangeException.ThrowIfLessThan(
            Encoding.UTF8.GetByteCount(secret),
            JwtOptions.MinimumSecretLength);

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
                    NameClaimType = JwtConstants.UniqueNameClaim,
                    RoleClaimType = JwtConstants.RoleClaim
                };
                jwt.Events = new JwtBearerEvents
                {
                    OnMessageReceived = ctx =>
                    {
                        ctx.Token ??= ctx.Request.Cookies[JwtConstants.AccessTokenCookieName];
                        return Task.CompletedTask;
                    }
                };
            });

        services.AddAuthorization(authorization =>
            authorization.AddPolicy(
                AuthorizationPolicies.Admin,
                policy => policy.RequireRole(AuthorizationPolicies.Admin)));

        return services;
    }
}
