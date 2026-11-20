using CoreSolution.Shared.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pandora.Api.Constants;
using Pandora.Api.Extensions.Http;
using Pandora.Api.Filters;
using Pandora.Application.Features.Settings.CredentialAccess;

namespace Pandora.Api.Endpoints;

public static class CredentialAccessSettingsEndpoints
{
    public static IEndpointRouteBuilder MapCredentialAccessSettingsEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup(ApiRoutes.AdminGroup).RequireAuthorization(nameof(UserRole.PandoraAdmin));

        group.MapGet(ApiRoutes.CredentialAccessSettings, Get);
        group.MapPut(ApiRoutes.CredentialAccessSettings, Update)
            .AddEndpointFilter<RequestContextFilter>()
            .AddEndpointFilter<ValidationFilter<UpdateCredentialAccessSettingsMessage>>();

        return endpoints;
    }

    private static async Task<IResult> Get(
        [FromServices] IMediator mediator,
        CancellationToken cancellationToken = default)
    {
        var result = await mediator.Send(new GetCredentialAccessSettingsMessage(), cancellationToken);

        return result.ToHttpResult();
    }

    private static async Task<IResult> Update(
        [FromServices] IMediator mediator,
        UpdateCredentialAccessSettingsMessage message,
        CancellationToken cancellationToken = default)
    {
        var result = await mediator.Send(message, cancellationToken);

        return result.ToHttpResult();
    }
}
