using CoreSolution.Shared.Enums;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Pandora.Api.Constants;
using Pandora.Api.Extensions.Http;
using Pandora.Api.Filters;
using Pandora.Application.Contracts;
using Pandora.Application.Features.Permissions;
using Pandora.Domain.Constants;

namespace Pandora.Api.Endpoints;

public static class EmployeeSecretsEndpoints
{
    public static IEndpointRouteBuilder MapEmployeeSecretsEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup(ApiRoutes.AdminGroup).RequireAuthorization(nameof(UserRole.PandoraAdmin));

        group.MapGet(ApiRoutes.EmployeeSecrets, List)
            .AddEndpointFilter<ValidationFilter<GetEmployeeSecretsRequest>>();
        group.MapPut(ApiRoutes.EmployeeSecretById, Grant)
            .AddEndpointFilter<RequestContextFilter>();
        group.MapDelete(ApiRoutes.EmployeeSecretById, Revoke)
            .AddEndpointFilter<RequestContextFilter>();

        return endpoints;
    }

    private static async Task<IResult> List(
        [FromServices] IMediator mediator,
        long telegramUserId,
        [AsParameters] GetEmployeeSecretsRequest request,
        CancellationToken cancellationToken = default)
    {
        var result = await mediator.Send(
            new GetEmployeeSecretsMessage(
                telegramUserId,
                request.Page ?? PaginationLimits.DefaultPage,
                request.PageSize ?? PaginationLimits.DefaultPageSize),
            cancellationToken);

        return result.ToHttpResult();
    }

    private static async Task<IResult> Grant(
        [FromServices] IMediator mediator,
        [AsParameters] GrantSecretAccessMessage message,
        CancellationToken cancellationToken = default)
    {
        var result = await mediator.Send(message, cancellationToken);

        return result.ToHttpResult();
    }

    private static async Task<IResult> Revoke(
        [FromServices] IMediator mediator,
        [AsParameters] RevokeSecretAccessMessage message,
        CancellationToken cancellationToken = default)
    {
        var result = await mediator.Send(message, cancellationToken);

        return result.ToHttpResult();
    }
}
