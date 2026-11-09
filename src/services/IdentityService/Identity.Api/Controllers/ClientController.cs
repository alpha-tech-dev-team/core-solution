using Identity.Api.Constants;
using Identity.Api.Extensions.Http;
using Identity.Application.Contracts.Client;
using Identity.Domain.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace Identity.Api.Controllers;

[ApiController]
[Route(ApiRoutes.ClientGroup)]
public sealed class ClientController(IIdentityService identityService) : ControllerBase
{
    [HttpPost(ApiRoutes.ActivateUser)]
    public async Task<IActionResult> ActivateUser(ActivateUserRequest request, CancellationToken cancellationToken)
    {
        var result = await identityService.ActivateUser(request.TelegramUserId, request.Phone, cancellationToken);

        return result.ToActionResult();
    }

    [HttpPost(ApiRoutes.CheckUser)]
    public async Task<IActionResult> CheckUser(CheckUserRequest request, CancellationToken cancellationToken)
    {
        var result = await identityService.CheckUser(request.TelegramUserId, request.Access, cancellationToken);

        return result.ToActionResult();
    }
}
