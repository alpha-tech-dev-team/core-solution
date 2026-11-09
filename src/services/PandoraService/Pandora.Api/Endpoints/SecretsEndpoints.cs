using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pandora.Api.Constants;
using Pandora.Api.Extensions;
using Pandora.Api.Filters;
using Pandora.Application.Contracts;
using Pandora.Application.Features.Secrets;
using Pandora.Domain.Constants;

namespace Pandora.Api.Endpoints;

public static class SecretsEndpoints
{
    public static IEndpointRouteBuilder MapSecretsEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup(ApiRoutes.AdminGroup).RequireAuthorization(AuthorizationPolicies.Admin);

        group.MapGet(ApiRoutes.Secrets, List)
            .AddEndpointFilter<ValidationFilter<GetSecretsRequest>>();
        group.MapPost(ApiRoutes.Secrets, Create)
            .AddEndpointFilter<ValidationFilter<CreateSecretRequest>>();
        group.MapPut(ApiRoutes.SecretById, Update)
            .AddEndpointFilter<ValidationFilter<UpdateSecretRequest>>();
        group.MapDelete(ApiRoutes.SecretById, Delete);

        return endpoints;
    }

    private static async Task<IResult> List(
        [FromServices] IMediator mediator,
        [AsParameters] GetSecretsRequest request,
        CancellationToken cancellationToken = default)
    {
        var result = await mediator.Send(
            new GetSecretsMessage(
                request.Page ?? PaginationLimits.DefaultPage,
                request.PageSize ?? PaginationLimits.DefaultPageSize),
            cancellationToken);

        return result.ToHttpResult();
    }

    private static async Task<IResult> Create(
        [FromServices] IMediator mediator,
        CreateSecretRequest request,
        HttpContext context,
        CancellationToken cancellationToken = default)
    {
        if (!TryGetAdminUserId(context, out var adminUserId))
        {
            return Results.Unauthorized();
        }

        var result = await mediator.Send(
            new CreateSecretMessage(adminUserId, request.Name, request.Username, request.Password),
            cancellationToken);

        return result.ToHttpResult();
    }

    private static async Task<IResult> Update(
        [FromServices] IMediator mediator,
        string itemId,
        UpdateSecretRequest request,
        HttpContext context,
        CancellationToken cancellationToken = default)
    {
        if (!TryGetAdminUserId(context, out var adminUserId))
        {
            return Results.Unauthorized();
        }

        var result = await mediator.Send(
            new UpdateSecretMessage(adminUserId, itemId, request.Name, request.Username, request.Password),
            cancellationToken);

        return result.ToHttpResult();
    }

    private static async Task<IResult> Delete(
        [FromServices] IMediator mediator,
        string itemId,
        HttpContext context,
        CancellationToken cancellationToken = default)
    {
        if (!TryGetAdminUserId(context, out var adminUserId))
        {
            return Results.Unauthorized();
        }

        var result = await mediator.Send(new DeleteSecretMessage(adminUserId, itemId), cancellationToken);

        return result.ToHttpResult();
    }

    private static bool TryGetAdminUserId(HttpContext context, out Guid adminUserId)
    {
        var subject = context.User.FindFirst(PandoraJwtConstants.SubjectClaim)?.Value;

        return Guid.TryParse(subject, out adminUserId);
    }
}
