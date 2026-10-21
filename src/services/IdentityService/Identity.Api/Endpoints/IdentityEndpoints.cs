using Identity.Api.Constants;
using Identity.Api.Filters;
using Identity.App.Contracts;
using Identity.Core.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace Identity.Api.Endpoints;

public static class IdentityEndpoints
{
    public static IEndpointRouteBuilder MapIdentityEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup(ApiRoutes.ClientGroup);

        group.MapPost(ApiRoutes.ActivateUser, ActivateUser)
            .AddEndpointFilter<ValidationFilter<ActivateUserRequest>>();

        group.MapPost(ApiRoutes.CheckUser, CheckUser)
            .AddEndpointFilter<ValidationFilter<CheckUserRequest>>();

        return endpoints;
    }

    private static async Task<IResult> ActivateUser(
        [FromServices] IIdentityService identityService,
        ActivateUserRequest request,
        CancellationToken cancellationToken = default)
    {
        var result = await identityService.ActivateUser(request.TelegramUserId, request.Phone, cancellationToken);

        return Results.Ok(result);
    }

    private static async Task<IResult> CheckUser(
        [FromServices] IIdentityService identityService,
        CheckUserRequest request,
        CancellationToken cancellationToken = default)
    {
        var result = await identityService.CheckUser(request.TelegramUserId, request.Access, cancellationToken);

        return Results.Ok(result);
    }
}
