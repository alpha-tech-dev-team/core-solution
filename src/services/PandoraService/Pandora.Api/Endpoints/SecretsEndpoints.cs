using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pandora.Api.Constants;
using Pandora.Api.Extensions;
using Pandora.Application.Contracts;
using Pandora.Domain.Interfaces.Services;

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

    private static async Task<IResult> List([FromServices] ISecretAdminService service, CancellationToken cancellationToken = default)
    {
        var result = await service.GetItems(cancellationToken);

        return result.ToHttpResult();
    }

    private static async Task<IResult> Create(
        [FromServices] ISecretAdminService service,
        CreateSecretRequest request,
        HttpContext context,
        CancellationToken cancellationToken = default)
    {
        var result = await service.Create(GetAdminUserId(context), request.Name, request.Username, request.Password, cancellationToken);

        return result.ToHttpResult();
    }

    private static async Task<IResult> Update(
        [FromServices] ISecretAdminService service,
        string itemId,
        UpdateSecretRequest request,
        HttpContext context,
        CancellationToken cancellationToken = default)
    {
        var result = await service.Update(GetAdminUserId(context), itemId, request.Name, request.Username, request.Password, cancellationToken);

        return result.ToHttpResult();
    }

    private static async Task<IResult> Delete(
        [FromServices] ISecretAdminService service,
        string itemId,
        HttpContext context,
        CancellationToken cancellationToken = default)
    {
        var result = await service.Delete(GetAdminUserId(context), itemId, cancellationToken);

        return result.ToHttpResult();
    }

    private static Guid GetAdminUserId(HttpContext context)
    {
        var subject = context.User.FindFirst(JwtConstants.SubjectClaim)?.Value;

        ArgumentException.ThrowIfNullOrEmpty(subject);

        return Guid.Parse(subject);
    }
}
