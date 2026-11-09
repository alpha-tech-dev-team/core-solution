using System.Text;
using CoreSolution.Shared.Constants;
using CoreSolution.Shared.Options;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using Pandora.Api.Authorization;
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
                jwt.MapInboundClaims = JwtValidationConstants.MapInboundClaims;
                jwt.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = JwtValidationConstants.ValidateIssuer,
                    ValidIssuer = options.Issuer,
                    ValidateAudience = JwtValidationConstants.ValidateAudience,
                    ValidAudience = options.Audience,
                    ValidateIssuerSigningKey = JwtValidationConstants.ValidateIssuerSigningKey,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret)),
                    ValidateLifetime = JwtValidationConstants.ValidateLifetime,
                    ClockSkew = JwtValidationConstants.ClockSkew,
                    ValidAlgorithms = JwtValidationConstants.ValidAlgorithms,
                    NameClaimType = PandoraJwtConstants.UniqueNameClaim,
                    RoleClaimType = PandoraJwtConstants.RoleClaim
                };
                jwt.Events = new JwtBearerEvents
                {
                    OnMessageReceived = ctx =>
                    {
                        ctx.Token ??= ctx.Request.Cookies[PandoraJwtConstants.AccessTokenCookieName];
                        return Task.CompletedTask;
                    }
                };
            });

        services.AddSingleton<IAuthorizationHandler, AdminUserIdAuthorizationHandler>();

        services.AddAuthorization(authorization =>
            authorization.AddPolicy(
                AuthorizationPolicies.Admin,
                policy =>
                {
                    policy.RequireRole(AuthorizationPolicies.Admin);
                    policy.Requirements.Add(new AdminUserIdRequirement());
                }));

        return services;
    }
}
