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
        _logger.LogInformation("StartRequest received for chat_id: {ChatId}", request.ChatId);
        
        var allowed = await _access.IsAllowedAsync(request.ChatId, ct);
        _logger.LogInformation("Access check result for chat_id {ChatId}: {Allowed}", request.ChatId, allowed);

        if (!allowed)
        {
            _logger.LogWarning("Access denied for chat_id {ChatId}", request.ChatId);
            return Result<TelegramMessageContent>.Failure("AccessDenied");
        }

        _logger.LogInformation("Access granted for chat_id {ChatId}, returning menu", request.ChatId);
        return Result<TelegramMessageContent>.Success(new TelegramMessageContent
        {
            Text = "Главное меню:",
            ButtonRows = [new TelegramButtonRow(
                new TelegramButton("Задачи", "menu:tasks"),
                new TelegramButton("Профиль", "menu:profile"))]
        });
    }
}