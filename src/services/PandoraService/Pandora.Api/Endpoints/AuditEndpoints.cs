using CoreSolution.Shared.Enums;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Pandora.Api.Constants;
using Pandora.Api.Extensions.Http;
using Pandora.Api.Filters;
using Pandora.Application.Features.Audit;

namespace Pandora.Api.Endpoints;

public static class AuditEndpoints
{
    public static IEndpointRouteBuilder MapAuditEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup(ApiRoutes.AdminGroup).RequireAuthorization(nameof(UserRole.PandoraAdmin));

        group.MapGet(ApiRoutes.Audit, GetAuditEntries)
            .AddEndpointFilter<ValidationFilter<GetAuditEntriesMessage>>();

        return endpoints;
    }

    private static async Task<IResult> GetAuditEntries(
        [FromServices] IMediator mediator,
        [AsParameters] GetAuditEntriesMessage message,
        CancellationToken cancellationToken = default)
    {
        var result = await mediator.Send(message, cancellationToken);

        return result.ToHttpResult();
    }
}
