using Atlas.Core.Enums;
using Atlas.Core.Interfaces;
using CoreSolution.Shared.Models;
using CoreSolution.Shared.Results;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Atlas.App.Features.Start;

public sealed class StartRequestHandler : IRequestHandler<StartRequest, Result<TelegramMessageContent>>
{
    private readonly IAccessService _access;
    private readonly ILogger<StartRequestHandler> _logger;

    public StartRequestHandler(IAccessService access, ILogger<StartRequestHandler> logger)
    {
        _access = access;
        _logger = logger;
    }

    public async Task<Result<TelegramMessageContent>> Handle(StartRequest request, CancellationToken ct)
    {
        _logger.LogInformation("StartRequest received");

        var allowed = await _access.IsAllowedAsync(request.TgUserId, ct);
        _logger.LogInformation("Access check result: {Allowed}", allowed);

        if (!allowed)
        {
            _logger.LogWarning("Access denied");
            return Result<TelegramMessageContent>.Failure(ErrorCode.AccessDenied.ToString());
        }

        _logger.LogInformation("Access granted, returning menu");
        return Result<TelegramMessageContent>.Success(new TelegramMessageContent
        {
            Text = StartConstants.MenuText,
            ButtonRows = [new TelegramButtonRow(
                new TelegramButton("Tasks", "menu:tasks"),
                new TelegramButton("Profile", "menu:profile"))]
        });
    }
}