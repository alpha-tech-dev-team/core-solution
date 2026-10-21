using Building.Telegram.Models;
using Building.Telegram.Results;
using MediatR;
using Pandora.App.Common;
using Pandora.Core.Interfaces;

namespace Pandora.App.Features.Start;

public sealed class StartHandler : IRequestHandler<StartMessage, Result<TelegramMessageContent>>
{
    private readonly IAccessService _accessService;

    public StartHandler(IAccessService accessService)
    {
        _accessService = accessService;
    }

    public async Task<Result<TelegramMessageContent>> Handle(StartMessage request, CancellationToken cancellationToken)
    {
        var services = await _accessService.GetServices(request.ChatId, cancellationToken);

        if (services.Count == 0)
        {
            return Result<TelegramMessageContent>.Success(new TelegramMessageContent { Text = StartConstants.NoServicesText });
        }

        var buttonRows = services
            .Select(service => new TelegramButtonRow(new TelegramButton(service, CallbackConstants.ServicePrefix + service)))
            .ToList();

        return Result<TelegramMessageContent>.Success(new TelegramMessageContent
        {
            Text = StartConstants.ServicesText,
            ButtonRows = buttonRows
        });
    }
}
