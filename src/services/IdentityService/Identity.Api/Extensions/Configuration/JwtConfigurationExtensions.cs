using System.Security.Claims;
using System.Text;
using CoreSolution.Shared.Enums;
using CoreSolution.Shared.Options;
using Identity.Api.Constants;
using Identity.Domain.Constants;
using Identity.Domain.Enums;
using Identity.Domain.Interfaces.Repositories;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using UserEntity = Identity.Domain.Entities.User;

namespace Identity.Api.Extensions.Configuration;

public static class JwtConfigurationExtensions
{
    private const string SecretEnvironmentVariable = "Jwt__Secret";

    public static IServiceCollection AddJwtAuthentication(this IServiceCollection services, IHostEnvironment environment)
    {
        if (environment.IsProduction()
            && string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable(SecretEnvironmentVariable)))
        {
            throw new InvalidOperationException(
                $"В Production секрет JWT задаётся только переменной окружения {SecretEnvironmentVariable}");
        }

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
                    },
                    OnTokenValidated = async ctx =>
                    {
                        var users = ctx.HttpContext.RequestServices.GetRequiredService<IUserRepository>();
                        var subject = ctx.Principal?.FindFirst(IdentityClaims.Subject)?.Value;

                        if (!Guid.TryParse(subject, out var userId))
                        {
                            ctx.Fail("В токене отсутствует идентификатор пользователя");
                            return;
                        }

                        var user = await users.GetById(
                            userId,
                            includeRoles: true,
                            cancellationToken: ctx.HttpContext.RequestAborted);

                        if (user is null || user.Status is UserStatus.Blocked or UserStatus.Archived)
                        {
                            ctx.Fail("Пользователь заблокирован или перенесен в архив");
                            return;
                        }

                        SyncClaims(ctx.Principal!, user);
                    }
                };
            });

        services.AddAuthorizationBuilder()
            .AddPolicy(
                nameof(UserRole.PandoraAdmin),
                policy => policy.RequireRole(nameof(UserRole.PandoraAdmin)));

        return services;
    }

    private static void SyncClaims(ClaimsPrincipal principal, UserEntity user)
    {
        if (principal.Identity is not ClaimsIdentity identity)
        {
            return;
        }

        foreach (var claim in identity.FindAll(IdentityClaims.Role).ToList())
        {
            identity.RemoveClaim(claim);
        }

        foreach (var role in user.UserRoles.Select(x => x.Role.Name).Distinct(StringComparer.OrdinalIgnoreCase))
        {
            identity.AddClaim(new Claim(IdentityClaims.Role, role));
        }

        var login = identity.FindFirst(IdentityClaims.UniqueName);
        if (login is not null)
        {
            identity.RemoveClaim(login);
        }

        if (user.AdminUsername is not null)
        {
            identity.AddClaim(new Claim(IdentityClaims.UniqueName, user.AdminUsername));
        }
    }
}
