using Atlas.Core.Interfaces;
using CoreSolution.Shared.Models;
using CoreSolution.Shared.Results;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Atlas.App.Features.Start;

public sealed class StartRequestHandler : IRequestHandler<StartRequest, Result<TelegramMessageContent>>
{
    private readonly ISessionService _sessionService;
    private readonly ILogger<StartRequestHandler> _logger;

    public StartRequestHandler(ISessionService sessionService, ILogger<StartRequestHandler> logger)
    {
        _sessionService = sessionService;
        _logger = logger;
    }

    public async Task<Result<TelegramMessageContent>> Handle(StartRequest request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("/start запрос получен", request.TgUserId);

        var session = await _sessionService.GetSession(request.TgUserId, cancellationToken);

        if (session is null)
        {
            _logger.LogInformation("Сесссия не найдена, тоебуется телефон", request.TgUserId);

            return Result<TelegramMessageContent>.Success(new TelegramMessageContent
            {
                Text = StartConstants.PhoneRequestText,
                RequestContact = true
            });
        }

        _logger.LogInformation("С", request.TgUserId);

        return Result<TelegramMessageContent>.Success(new TelegramMessageContent
        {
            Text = StartConstants.MenuText,
            ButtonRows =
            [
                new TelegramButtonRow(
                    new TelegramButton(StartConstants.Problems, "menu:problems"),
                    new TelegramButton(StartConstants.Profile, "menu:profile"))
            ]
        });
    }
}
