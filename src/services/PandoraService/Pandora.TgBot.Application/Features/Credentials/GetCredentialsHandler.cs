using CoreSolution.Shared.Abstractions;
using CoreSolution.Shared.Models;
using CoreSolution.Shared.Results;
using MediatR;
using Microsoft.Extensions.Logging;
using Pandora.Domain.Constants.Localization;
using Pandora.Domain.Enums;
using Pandora.Domain.Interfaces;

namespace Pandora.TgBot.Application.Features.Credentials;

public sealed class GetCredentialsHandler : IRequestHandler<GetCredentialsMessage, Result<WorkflowResponse>>
{
    private readonly ISecretStore _secretStore;
    private readonly IPermissionRepository _permissionRepository;
    private readonly ILocalizer _localizer;
    private readonly ILogger<GetCredentialsHandler> _logger;

    public GetCredentialsHandler(ISecretStore secretStore, IPermissionRepository permissionRepository, ILocalizer localizer, ILogger<GetCredentialsHandler> logger)
    {
        _secretStore = secretStore;
        _permissionRepository = permissionRepository;
        _localizer = localizer;
        _logger = logger;
    }

    public async Task<Result<WorkflowResponse>> Handle(GetCredentialsMessage request, CancellationToken cancellationToken)
    {
        var hasAccess = await _permissionRepository.HasAccess(request.TelegramUserId, request.ItemId, cancellationToken);

        if (!hasAccess)
        {
            _logger.LogWarning("Отказано в выдаче секрета {ItemId}, сообщение {MessageId}", request.ItemId, request.MessageId);

            return Result<WorkflowResponse>.Failure(ErrorCode.AccessDenied.ToString());
        }

        var secretResult = await _secretStore.GetSecret(request.ItemId, cancellationToken);
        var secret = secretResult.Value;

        if (secret is null)
        {
            return Result<WorkflowResponse>.Failure(secretResult.ErrorCode ?? string.Empty);
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
