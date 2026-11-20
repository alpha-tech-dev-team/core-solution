using Identity.Api.Constants;
using Identity.Api.Extensions.Http;
using Identity.Api.Middleware;
using Identity.Application.Contracts.Client;
using Identity.Domain.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Options;

namespace Identity.Api.Controllers;

[ApiController]
[Route(ApiRoutes.ClientGroup)]
public sealed class ClientController : ControllerBase
{
    private readonly IIdentityService _identityService;
    private readonly IOptions<ClientApiOptions> _clientApiOptions;

    public ClientController(IIdentityService identityService, IOptions<ClientApiOptions> clientApiOptions)
    {
        _identityService = identityService;
        _clientApiOptions = clientApiOptions;
    }

    [HttpPost(ApiRoutes.ActivateUser)]
    [EnableRateLimiting(RateLimitingPolicies.BotClient)]
    public async Task<IActionResult> ActivateUser(ActivateUserRequest request, CancellationToken cancellationToken)
    {
        var result = await _identityService.ActivateUser(request.TelegramUserId, request.Phone, cancellationToken);

        return result.ToActionResult();
    }

    [HttpPost(ApiRoutes.CheckUser)]
    [EnableRateLimiting(RateLimitingPolicies.BotClient)]
    public async Task<IActionResult> CheckUser(CheckUserRequest request, CancellationToken cancellationToken)
    {
        var result = await _identityService.CheckUser(request.TelegramUserId, request.Access, cancellationToken);

        return result.ToActionResult();
    }

    [HttpGet(ApiRoutes.Employee)]
    [EnableRateLimiting(RateLimitingPolicies.EmployeeDirectory)]
    public async Task<IActionResult> GetEmployee([FromRoute] GetEmployeeRequest request, CancellationToken cancellationToken)
    {
        if (!ClientApiKeyValidator.IsValid(Request, _clientApiOptions.Value))
        {
            return ClientApiKeyValidator.Unauthorized();
        }

        var result = await _identityService.GetEmployee(request.TelegramUserId, cancellationToken);

        return result.ToActionResult();
    }
}
