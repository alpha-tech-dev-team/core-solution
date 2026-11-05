using System.Text;
using CoreSolution.Shared.Options;
using Identity.Api.Constants;
using Identity.Domain.Constants;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Identity.Api.Extensions;

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

                jwt.MapInboundClaims = JwtConstants.MapInboundClaims;
                jwt.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = JwtConstants.ValidateIssuer,
                    ValidIssuer = options.Issuer,
                    ValidateAudience = JwtConstants.ValidateAudience,
                    ValidAudience = options.Audience,
                    ValidateIssuerSigningKey = JwtConstants.ValidateIssuerSigningKey,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(options.Secret)),
                    ValidateLifetime = JwtConstants.ValidateLifetime,
                    ClockSkew = JwtConstants.ClockSkew,
                    ValidAlgorithms = JwtConstants.ValidAlgorithms,
                    NameClaimType = IdentityClaims.UniqueName,
                    RoleClaimType = IdentityClaims.Role
                };
                jwt.Events = new JwtBearerEvents
                {
                    OnMessageReceived = ctx =>
                    {
                        ctx.Token ??= ctx.Request.Cookies[AuthCookieConstants.AccessTokenName];
                        return Task.CompletedTask;
                    }
                };
            });

        services.AddAuthorizationBuilder()
            .AddPolicy(
                AuthorizationPolicies.Admin,
                policy => policy.RequireRole(AuthorizationPolicies.Admin));

        return services;
    }
}
