using FluentValidation;
using Identity.Api.Extensions;
using Identity.App.Contracts;
using Identity.Core.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace Identity.Api.Endpoints;

public static class IdentityEndpoints
{
    public static IEndpointRouteBuilder MapIdentityEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/client");

        group.MapPost("activate", ActivateUser);
        group.MapPost("check", CheckUser);

        return endpoints;
    }

    private static async Task<IResult> ActivateUser(
        [FromServices] IIdentityService identityService,
        [FromServices] IValidator<ActivateUserRequest> validator,
        ActivateUserRequest request,
        CancellationToken cancellationToken = default)
    {
        var validation = await validator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid)
        {
            return validation.ToValidationProblem();
        }

        var result = await identityService.ActivateUser(request.TelegramUserId, request.Phone, cancellationToken);

        return Results.Ok(result);
    }

    private static async Task<IResult> CheckUser(
        [FromServices] IIdentityService identityService,
        [FromServices] IValidator<CheckUserRequest> validator,
        CheckUserRequest request,
        CancellationToken cancellationToken = default)
    {
        var validation = await validator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid)
        {
            return validation.ToValidationProblem();
        }

        var result = await identityService.CheckUser(request.TelegramUserId, request.Access, cancellationToken);

        return Results.Ok(result);
    }
}
