using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pandora.Api.Constants;
using Pandora.Api.Extensions;
using Pandora.Application.Contracts;
using Pandora.Application.Features.Secrets;

namespace Pandora.Api.Endpoints;

public static class SecretsEndpoints
{
    public static IEndpointRouteBuilder MapSecretsEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup(ApiRoutes.AdminGroup).RequireAuthorization(AuthorizationPolicies.Admin);

        group.MapGet(ApiRoutes.Secrets, List);
        group.MapPost(ApiRoutes.Secrets, Create);
        group.MapPut(ApiRoutes.SecretById, Update);
        group.MapDelete(ApiRoutes.SecretById, Delete);

        return endpoints;
    }

    private static async Task<IResult> List([FromServices] IMediator mediator, CancellationToken cancellationToken = default)
    {
        var result = await mediator.Send(new GetSecretsQuery(), cancellationToken);

        return result.ToHttpResult();
    }

    private static async Task<IResult> Create(
        [FromServices] IMediator mediator,
        [FromServices] IValidator<CreateSecretRequest> validator,
        CreateSecretRequest request,
        HttpContext context,
        CancellationToken cancellationToken = default)
    {
        var validationProblem = ToValidationProblem(await validator.ValidateAsync(request, cancellationToken));

        if (validationProblem is not null)
        {
            return validationProblem;
        }

        var result = await mediator.Send(
            new CreateSecretCommand(GetAdminUserId(context), request.Name, request.Username, request.Password),
            cancellationToken);

        return result.ToHttpResult();
    }

    private static async Task<IResult> Update(
        [FromServices] IMediator mediator,
        [FromServices] IValidator<UpdateSecretRequest> validator,
        string itemId,
        UpdateSecretRequest request,
        HttpContext context,
        CancellationToken cancellationToken = default)
    {
        var validationProblem = ToValidationProblem(await validator.ValidateAsync(request, cancellationToken));

        if (validationProblem is not null)
        {
            return validationProblem;
        }

        var result = await mediator.Send(
            new UpdateSecretCommand(GetAdminUserId(context), itemId, request.Name, request.Username, request.Password),
            cancellationToken);

        return result.ToHttpResult();
    }

    private static async Task<IResult> Delete(
        [FromServices] IMediator mediator,
        string itemId,
        HttpContext context,
        CancellationToken cancellationToken = default)
    {
        var result = await mediator.Send(new DeleteSecretCommand(GetAdminUserId(context), itemId), cancellationToken);

        return result.ToHttpResult();
    }

    private static Guid GetAdminUserId(HttpContext context)
    {
        var subject = context.User.FindFirst(JwtConstants.SubjectClaim)?.Value;

        ArgumentException.ThrowIfNullOrEmpty(subject);

        return Guid.Parse(subject);
    }

    private static IResult? ToValidationProblem(ValidationResult validationResult)
    {
        if (validationResult.IsValid)
        {
            return null;
        }

        var errors = validationResult.Errors
            .GroupBy(error => error.PropertyName)
            .ToDictionary(group => group.Key, group => group.Select(error => error.ErrorMessage).ToArray());

        return Results.ValidationProblem(errors);
    }
}
