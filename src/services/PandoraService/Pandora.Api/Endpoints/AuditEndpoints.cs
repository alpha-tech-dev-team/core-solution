using MediatR;
using Microsoft.AspNetCore.Mvc;
using CoreSolution.Shared.Enums;
using Pandora.Api.Constants;
using Pandora.Api.Extensions;
using Pandora.Api.Filters;
using Pandora.Application.Contracts;
using Pandora.Application.Features.Audit;
using Pandora.Domain.Constants;

namespace Pandora.Api.Endpoints;

public static class AuditEndpoints
{
    public static IEndpointRouteBuilder MapAuditEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup(ApiRoutes.AdminGroup).RequireAuthorization(nameof(UserRole.PandoraAdmin));

        group.MapGet(ApiRoutes.Audit, List)
            .AddEndpointFilter<ValidationFilter<GetAuditEntriesRequest>>();

        return endpoints;
    }

    private static async Task<IResult> List(
        [FromServices] IMediator mediator,
        [AsParameters] GetAuditEntriesRequest request,
        CancellationToken cancellationToken = default)
    {
        var result = await mediator.Send(
            new GetAuditEntriesMessage(
                request.Page ?? PaginationLimits.DefaultPage,
                request.PageSize ?? PaginationLimits.DefaultPageSize),
            cancellationToken);

        return result.ToHttpResult();
    }
}
