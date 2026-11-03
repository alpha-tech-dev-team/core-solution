using CoreSolution.Shared.Abstractions;
using CoreSolution.Shared.Models;
using CoreSolution.Shared.Results;
using MediatR;
using Pandora.Domain.Constants.Localization;
using Pandora.Domain.Enums;
using Pandora.Domain.Interfaces;

namespace Pandora.Application.Features.Credentials;

public sealed class GetCredentialsHandler : IRequestHandler<GetCredentialsMessage, Result<WorkflowResponse>>
{
    private readonly ISecretStore _secretStore;
    private readonly IPermissionRepository _permissionRepository;
    private readonly ILocalizer _localizer;

    public GetCredentialsHandler(ISecretStore secretStore, IPermissionRepository permissionRepository, ILocalizer localizer)
    {
        _secretStore = secretStore;
        _permissionRepository = permissionRepository;
        _localizer = localizer;
    }

    public async Task<Result<WorkflowResponse>> Handle(GetCredentialsMessage request, CancellationToken cancellationToken)
    {
        var hasAccess = await _permissionRepository.HasAccess(request.TelegramUserId, request.ItemId, cancellationToken);

        if (!hasAccess)
        {
            return Result<WorkflowResponse>.Failure(ErrorCode.AccessDenied.ToString());
        }

        var secret = await _secretStore.GetSecret(request.ItemId, cancellationToken);

        if (secret is null)
        {
            return Result<WorkflowResponse>.Failure(ErrorCode.SecretNotFound.ToString());
        }

        var text = string.Format(
            _localizer.GetPhrase(nameof(WorkflowStep.ServiceSelection), LocalizationKeysConstants.ServiceSelection.CredentialsTemplate),
            secret.ItemName,
            _localizer.GetPhrase(nameof(WorkflowStep.ServiceSelection), LocalizationKeysConstants.ServiceSelection.LoginLabel),
            secret.Login,
            _localizer.GetPhrase(nameof(WorkflowStep.ServiceSelection), LocalizationKeysConstants.ServiceSelection.PasswordLabel),
            secret.Password);

        return Result<WorkflowResponse>.Success(new WorkflowResponse { Text = text });
    }
}
