using Atlas.Api.Constants;
using Atlas.Api.Extensions.Http;
using Atlas.Api.Filters;
using Atlas.Application.Features.Incidents;
using CoreSolution.Shared.Enums;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Atlas.Api.Endpoints;

public static class IncidentsEndpoints
{
    public static IEndpointRouteBuilder MapIncidentsEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup(ApiRoutes.AdminGroup).RequireAuthorization(nameof(UserRole.AtlasAdmin));

        group.MapGet(ApiRoutes.Incidents, List)
            .AddEndpointFilter<ValidationFilter<GetIncidentsMessage>>();

        return endpoints;
    }

    private static async Task<IResult> List(
        [FromServices] IMediator mediator,
        [AsParameters] GetIncidentsMessage message,
        CancellationToken cancellationToken = default)
    {
        var result = await mediator.Send(message, cancellationToken);

        return result.ToHttpResult();
    }
}
