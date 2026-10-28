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
        _logger.LogInformation($"/start запрос получен от {request.TgUserId}", request.TgUserId);

        var session = await _sessionService.GetSession(request.TgUserId, cancellationToken);

        if (session is null)
        {
            _logger.LogInformation($"Сессия не найдена, требуется телефон пользователя {request.TgUserId}", request.TgUserId);

            return Result<TelegramMessageContent>.Success(new TelegramMessageContent
            {
                Text = StartConstants.PhoneRequestText,
                RequestContact = StartConstants.DefaultRequestContact
            });
        }

        _logger.LogInformation($"Сессия пользователя {request.TgUserId} найдена", request.TgUserId);

        return Result<TelegramMessageContent>.Success(new TelegramMessageContent
        {
            Text = StartConstants.MenuText,
            ButtonRows =
            [
                new TelegramButtonRow(
                    new TelegramButton(StartConstants.Problems, StartConstants.MenuProblems),
                    new TelegramButton(StartConstants.Profile, StartConstants.MenuProfile))
            ]
        });
    }
}
