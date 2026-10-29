using Atlas.Core.Entities;
using Atlas.Core.Enums;
using Atlas.Core.Interfaces;
using CoreSolution.Shared.Models;
using CoreSolution.Shared.Results;
using MediatR;

namespace Atlas.App.Features.Activation;

public sealed class SubmitPhoneHandler : IRequestHandler<SubmitPhoneMessage, Result<TelegramMessageContent>>
{
    private const string InternationalPrefix = "+";

    private readonly IIdentityClient _identityClient;
    private readonly ISessionService _sessionService;

    public SubmitPhoneHandler(IIdentityClient identityClient, ISessionService sessionService)
    {
        _identityClient = identityClient;
        _sessionService = sessionService;
    }

    public async Task<Result<TelegramMessageContent>> Handle(SubmitPhoneMessage request, CancellationToken cancellationToken)
    {
        var approved = await _identityClient.ActivateUser(
            request.TelegramUserId,
            NormalizePhone(request.Phone),
            cancellationToken);

        if (!approved)
        {
            return Result<TelegramMessageContent>.Failure(ErrorCode.AccessNotConfirmed.ToString());
        }

        var session = new UserSession(
            request.TelegramUserId,
            WorkflowStep.ProblemCategorySelection,
            Language.Ru,
            [],
            DateTime.UtcNow);

        await _sessionService.SaveSession(session, cancellationToken);

        return Result<TelegramMessageContent>.Success(new TelegramMessageContent
        {
            Text = string.Empty
        });
    }

    private static string NormalizePhone(string phone)
    {
        var digits = new string(phone.Where(char.IsDigit).ToArray());

        return $"{InternationalPrefix}{digits}";
    }
}
