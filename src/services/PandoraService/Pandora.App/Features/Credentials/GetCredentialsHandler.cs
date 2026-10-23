using CoreSolution.Shared.Models;
using CoreSolution.Shared.Results;
using MediatR;
using Pandora.Core.Constants.Localization;
using Pandora.Core.Enums;
using Pandora.Core.Interfaces;

namespace Pandora.App.Features.Credentials;

public sealed class GetCredentialsHandler : IRequestHandler<GetCredentialsMessage, Result<TelegramMessageContent>>
{
    private readonly IRequestContext _requestContext;
    private readonly ISecretStore _secretStore;
    private readonly ILocalizer _localizer;

    public GetCredentialsHandler(IRequestContext requestContext, ISecretStore secretStore, ILocalizer localizer)
    {
        _requestContext = requestContext;
        _secretStore = secretStore;
        _localizer = localizer;
    }

    public async Task<Result<TelegramMessageContent>> Handle(GetCredentialsMessage request, CancellationToken cancellationToken)
    {
        if (!_requestContext.Session.Roles.Contains(request.ServiceName))
        {
            return Result<TelegramMessageContent>.Failure(ErrorCode.AccessDenied.ToString());
        }

        var secret = await _secretStore.GetSecret(request.ServiceName, cancellationToken);

        if (secret is null)
        {
            return Result<TelegramMessageContent>.Failure(ErrorCode.SecretNotFound.ToString());
        }

        var text = string.Format(
            _localizer.GetPhrase(WorkflowStep.ServiceSelection, LocalizationKeysConstants.ServiceSelection.CredentialsTemplate),
            request.ServiceName,
            _localizer.GetPhrase(WorkflowStep.ServiceSelection, LocalizationKeysConstants.ServiceSelection.LoginLabel),
            secret.Login,
            _localizer.GetPhrase(WorkflowStep.ServiceSelection, LocalizationKeysConstants.ServiceSelection.PasswordLabel),
            secret.Password);

        return Result<TelegramMessageContent>.Success(new TelegramMessageContent { Text = text });
    }
}
