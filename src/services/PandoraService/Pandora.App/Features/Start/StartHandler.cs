using CoreSolution.Shared.Models;
using CoreSolution.Shared.Results;
using MediatR;
using Pandora.App.Common;
using Pandora.Core.Enums;
using Pandora.Core.Interfaces;

namespace Pandora.App.Features.Start;

public sealed class StartHandler : IRequestHandler<StartMessage, Result<TelegramMessageContent>>
{
    private readonly IAccessService _accessService;
    private readonly IRequestContext _requestContext;
    private readonly ISessionService _sessionService;

    public StartHandler(IAccessService accessService, IRequestContext requestContext, ISessionService sessionService)
    {
        _accessService = accessService;
        _requestContext = requestContext;
        _sessionService = sessionService;
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

        _requestContext.Session.State = WorkflowStep.ServiceSelection;
        await _sessionService.SaveSession(_requestContext.Session, cancellationToken);

        return Result<TelegramMessageContent>.Success(new TelegramMessageContent
        {
            Text = StartConstants.ServicesText,
            ButtonRows = buttonRows
        });
    }
}
