using CoreSolution.Shared.Enums;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Pandora.Api.Constants;
using Pandora.Api.Extensions.Http;
using Pandora.Api.Filters;
using Pandora.Application.Features.Permissions;

namespace Pandora.Api.Endpoints;

public static class EmployeeSecretsEndpoints
{
    public static IEndpointRouteBuilder MapEmployeeSecretsEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup(ApiRoutes.AdminGroup).RequireAuthorization(nameof(UserRole.PandoraAdmin));

        group.MapGet(ApiRoutes.EmployeeSecrets, List)
            .AddEndpointFilter<ValidationFilter<GetEmployeeSecretsMessage>>();
        group.MapPut(ApiRoutes.EmployeeSecretById, Grant)
            .AddEndpointFilter<RequestContextFilter>()
            .AddEndpointFilter<ValidationFilter<GrantSecretAccessMessage>>();
        group.MapDelete(ApiRoutes.EmployeeSecretById, Revoke)
            .AddEndpointFilter<RequestContextFilter>()
            .AddEndpointFilter<ValidationFilter<RevokeSecretAccessMessage>>();

        return endpoints;
    }

    private static async Task<IResult> List(
        [FromServices] IMediator mediator,
        [AsParameters] GetEmployeeSecretsMessage message,
        CancellationToken cancellationToken = default)
    {
        var result = await mediator.Send(message, cancellationToken);

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
