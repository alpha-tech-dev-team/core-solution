using CoreSolution.Shared.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pandora.Api.Constants;
using Pandora.Api.Extensions.Http;
using Pandora.Api.Filters;
using Pandora.Application.Features.Secrets.CreateSecret;
using Pandora.Application.Features.Secrets.DeleteSecret;
using Pandora.Application.Features.Secrets.GetSecrets;
using Pandora.Application.Features.Secrets.UpdateSecret;

namespace Pandora.Api.Endpoints;

public static class SecretsEndpoints
{
    public static IEndpointRouteBuilder MapSecretsEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup(ApiRoutes.AdminGroup).RequireAuthorization(nameof(UserRole.PandoraAdmin));

        group.MapGet(ApiRoutes.Secrets, List)
            .AddEndpointFilter<ValidationFilter<GetSecretsMessage>>();
        group.MapPost(ApiRoutes.Secrets, Create)
            .AddEndpointFilter<RequestContextFilter>()
            .AddEndpointFilter<ValidationFilter<CreateSecretMessage>>();
        group.MapPatch(ApiRoutes.SecretById, Update)
            .AddEndpointFilter<RequestContextFilter>()
            .AddEndpointFilter<ValidationFilter<UpdateSecretMessage>>();
        group.MapDelete(ApiRoutes.SecretById, Delete)
            .AddEndpointFilter<RequestContextFilter>()
            .AddEndpointFilter<ValidationFilter<DeleteSecretMessage>>();

        return endpoints;
    }

    private static async Task<IResult> List(
        [FromServices] IMediator mediator,
        [AsParameters] GetSecretsMessage message,
        CancellationToken cancellationToken = default)
    {
        var result = await mediator.Send(message, cancellationToken);

        return result.ToHttpResult();
    }

    private static async Task<IResult> Create(
        [FromServices] IMediator mediator,
        CreateSecretMessage message,
        CancellationToken cancellationToken = default)
    {
        var result = await mediator.Send(message, cancellationToken);

        return result.ToHttpResult();
    }

    private static async Task<IResult> Update(
        [FromServices] IMediator mediator,
        UpdateSecretMessage message,
        CancellationToken cancellationToken = default)
    {
        var result = await mediator.Send(message, cancellationToken);

        return result.ToHttpResult();
    }

    private static async Task<IResult> Delete(
        [FromServices] IMediator mediator,
        [AsParameters] DeleteSecretMessage message,
        CancellationToken cancellationToken = default)
    {
        var result = await mediator.Send(message, cancellationToken);

        return result.ToHttpResult();
    }
}
