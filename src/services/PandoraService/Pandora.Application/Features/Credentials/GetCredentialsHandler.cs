using CoreSolution.Shared.Abstractions;
using CoreSolution.Shared.Models;
using CoreSolution.Shared.Results;
using MediatR;
using Pandora.Core.Constants.Localization;
using Pandora.Core.Enums;
using Pandora.Core.Interfaces;

namespace Pandora.Application.Features.Credentials;

public sealed class GetCredentialsHandler : IRequestHandler<GetCredentialsMessage, Result<WorkflowResponse>>
{
    private readonly ISecretStore _secretStore;
    private readonly ILocalizer _localizer;

    public GetCredentialsHandler(ISecretStore secretStore, ILocalizer localizer)
    {
        _secretStore = secretStore;
        _localizer = localizer;
    }

    public async Task<Result<WorkflowResponse>> Handle(GetCredentialsMessage request, CancellationToken cancellationToken)
    {
        var secret = await _secretStore.GetSecret(request.ServiceName, cancellationToken);

        if (secret is null)
        {
            return Result<WorkflowResponse>.Failure(ErrorCode.SecretNotFound.ToString());
        }

        var text = string.Format(
            _localizer.GetPhrase(nameof(WorkflowStep.ServiceSelection), LocalizationKeysConstants.ServiceSelection.CredentialsTemplate),
            request.ServiceName,
            _localizer.GetPhrase(nameof(WorkflowStep.ServiceSelection), LocalizationKeysConstants.ServiceSelection.LoginLabel),
            secret.Login,
            _localizer.GetPhrase(nameof(WorkflowStep.ServiceSelection), LocalizationKeysConstants.ServiceSelection.PasswordLabel),
            secret.Password);

        return Result<WorkflowResponse>.Success(new WorkflowResponse { Text = text });
    }
}
