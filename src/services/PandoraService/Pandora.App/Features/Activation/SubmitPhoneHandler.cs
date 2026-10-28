using CoreSolution.Shared.Models;
using CoreSolution.Shared.Results;
using MediatR;
using Pandora.App.Common;
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
    private readonly ISecretStore _secretStore;
    private readonly ILocalizer _localizer;

    public SubmitPhoneHandler(IIdentityClient identityClient, ISessionService sessionService, ISecretStore secretStore, ILocalizer localizer)
    {
        _identityClient = identityClient;
        _sessionService = sessionService;
        _secretStore = secretStore;
        _localizer = localizer;
    }

    public async Task<Result<TelegramMessageContent>> Handle(SubmitPhoneMessage request, CancellationToken cancellationToken)
    {
        if (request.Phone is not null)
        {
            var approved = await _identityClient.ActivateUser(request.TelegramUserId, NormalizePhone(request.Phone), cancellationToken);

            if (!approved)
            {
                return Result<TelegramMessageContent>.Failure(ErrorCode.AccessNotConfirmed.ToString());
            }
        }

        var session = new UserSession(
            request.TelegramUserId,
            WorkflowStep.ServiceSelection,
            LocalizationConstants.DefaultLanguage,
            DateTime.UtcNow);

        await _sessionService.SaveSession(session, cancellationToken);

        var services = _secretStore.GetServiceNames();

        if (services.Count == 0)
        {
            return Result<TelegramMessageContent>.Success(new TelegramMessageContent
            {
                Text = _localizer.GetPhrase(WorkflowStep.Idle, LocalizationKeysConstants.Idle.NoServices)
            });
        }

        return Result<TelegramMessageContent>.Success(ServiceMenu.Create(services, _localizer));
    }

    private static string NormalizePhone(string phone)
    {
        var digits = new string(phone.Where(char.IsDigit).ToArray());

        return $"{InternationalPrefix}{digits}";
    }
}
