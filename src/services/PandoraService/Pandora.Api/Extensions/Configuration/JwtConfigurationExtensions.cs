using System.Text;
using CoreSolution.Shared.Constants;
using CoreSolution.Shared.Enums;
using CoreSolution.Shared.Options;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Pandora.Api.Extensions.Configuration;

public static class JwtConfigurationExtensions
{
    public static IServiceCollection AddJwtAuthentication(this IServiceCollection services)
    {
        services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer();

        services.AddOptions<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme)
            .Configure<IOptions<JwtOptions>>((jwt, jwtOptionsAccessor) =>
            {
                var options = jwtOptionsAccessor.Value;
                var secret = options.Secret;

                ArgumentException.ThrowIfNullOrWhiteSpace(secret);
                ArgumentOutOfRangeException.ThrowIfLessThan(
                    Encoding.UTF8.GetByteCount(secret),
                    JwtOptions.MinimumSecretLength);

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
                    NameClaimType = JwtClaimConstants.UniqueName,
                    RoleClaimType = JwtClaimConstants.Role
                };
                jwt.Events = new JwtBearerEvents
                {
                    OnMessageReceived = context =>
                    {
                        context.Token ??= context.Request.Cookies[AuthCookieConstants.AccessTokenName];
                        return Task.CompletedTask;
                    }
                };
            });

        services.AddAuthorization(authorization =>
            authorization.AddPolicy(
                nameof(UserRole.PandoraAdmin),
                policy => policy
                    .RequireRole(nameof(UserRole.PandoraAdmin))
                    .RequireClaim(JwtClaimConstants.Subject)));

        return services;
    }
}
