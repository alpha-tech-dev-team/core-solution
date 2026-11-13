using System.Text;
using CoreSolution.Shared.Enums;
using CoreSolution.Shared.Options;
using Identity.Api.Constants;
using Identity.Api.Models;
using Identity.Api.Services;
using Identity.Application.Errors;
using Identity.Domain.Constants;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Identity.Api.Extensions.Configuration;

public static class JwtConfigurationExtensions
{
    private const string SecretEnvironmentVariable = "Jwt__Secret";
    private const string AuthenticationErrorItemKey = "AuthenticationErrorCode";

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
                        var validator = ctx.HttpContext.RequestServices.GetRequiredService<AdminSessionValidator>();
                        var errorCode = await validator.ValidateAsync(ctx.Principal!, ctx.HttpContext.RequestAborted);

                        if (errorCode is null)
                        {
                            return;
                        }

                        ctx.HttpContext.Items[AuthenticationErrorItemKey] = errorCode;
                        ctx.Fail(errorCode);
                    },
                    OnChallenge = async ctx =>
                    {
                        var errorCode = ctx.HttpContext.Items[AuthenticationErrorItemKey] as string
                            ?? nameof(IdentityErrorCode.InvalidCredentials);

                        ctx.HandleResponse();
                        await WriteErrorAsync(ctx.Response, StatusCodes.Status401Unauthorized, errorCode);
                    },
                    OnForbidden = ctx =>
                    {
                        return WriteErrorAsync(
                            ctx.Response,
                            StatusCodes.Status403Forbidden,
                            nameof(IdentityErrorCode.AccessDenied));
                    }
                };
            });

        services.AddAuthorizationBuilder()
            .AddPolicy(
                nameof(UserRole.PandoraAdmin),
                policy => policy.RequireRole(nameof(UserRole.PandoraAdmin)));

        return services;
    }

    private static Task WriteErrorAsync(HttpResponse response, int statusCode, string errorCode)
    {
        response.StatusCode = statusCode;

        return response.WriteAsJsonAsync(new ErrorResponse(errorCode));
    }
}
