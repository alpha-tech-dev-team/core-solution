using CoreSolution.Shared.Models;
using CoreSolution.Shared.Results;
using MediatR;
using Pandora.App.Features.Start;
using Pandora.Core.Constants.Localization;
using Pandora.Core.Entities;
using Pandora.Core.Enums;
using Pandora.Core.Interfaces;

namespace Pandora.App.Features.Activation;

public sealed class SubmitPhoneHandler : IRequestHandler<SubmitPhoneMessage, Result<TelegramMessageContent>>
{
    private const string InternationalPrefix = "+";

    private readonly IIdentityClient _identityClient;
    private readonly ISessionService _sessionService;
    private readonly IMediator _mediator;

    public SubmitPhoneHandler(IIdentityClient identityClient, ISessionService sessionService, IMediator mediator)
    {
        _identityClient = identityClient;
        _sessionService = sessionService;
        _mediator = mediator;
    }

    public async Task<Result<TelegramMessageContent>> Handle(SubmitPhoneMessage request, CancellationToken cancellationToken)
    {
        var approved = await _identityClient.ActivateUser(request.TelegramUserId, NormalizePhone(request.Phone), cancellationToken);

        if (!approved)
        {
            return Result<TelegramMessageContent>.Failure(ErrorCode.AccessNotConfirmed.ToString());
        }

        var session = new UserSession(
            request.TelegramUserId,
            WorkflowStep.Idle,
            LocalizationConstants.DefaultLanguage,
            [],
            DateTime.UtcNow);

        await _sessionService.SaveSession(session, cancellationToken);

        return await _mediator.Send(new StartMessage(request.TelegramUserId, request.MessageId), cancellationToken);
    }

    private static string NormalizePhone(string phone)
    {
        var digits = new string(phone.Where(char.IsDigit).ToArray());

        return $"{InternationalPrefix}{digits}";
    }
}
