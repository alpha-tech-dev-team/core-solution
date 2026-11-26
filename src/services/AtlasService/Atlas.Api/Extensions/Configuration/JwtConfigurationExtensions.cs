using System.Text;
using CoreSolution.Shared.Constants;
using CoreSolution.Shared.Enums;
using CoreSolution.Shared.Options;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Atlas.Api.Extensions.Configuration;

public static class JwtConfigurationExtensions
{
    public static IServiceCollection AddJwtAuthentication(this IServiceCollection services, IHostEnvironment environment)
    {
        var appSettingsSecret = ReadAppSettingsSecret(environment);

        services.AddOptions<JwtOptions>()
            .Validate(
                options => !environment.IsProduction()
                    || !string.Equals(options.Secret, appSettingsSecret, StringComparison.Ordinal),
                "Jwt:Secret must not use the appsettings.json value in Production.")
            .ValidateOnStart();

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

                if (environment.IsProduction()
                    && string.Equals(secret, appSettingsSecret, StringComparison.Ordinal))
                {
                    throw new InvalidOperationException(
                        "Jwt:Secret must not use the appsettings.json value in Production.");
                }

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
                nameof(UserRole.AtlasAdmin),
                policy => policy
                    .RequireRole(nameof(UserRole.AtlasAdmin))
                    .RequireClaim(JwtClaimConstants.Subject)));

        return services;
    }

    private static string? ReadAppSettingsSecret(IHostEnvironment environment)
    {
        return new ConfigurationBuilder()
            .SetBasePath(environment.ContentRootPath)
            .AddJsonFile("appsettings.json", optional: true)
            .Build()["Jwt:Secret"];
    }
}
