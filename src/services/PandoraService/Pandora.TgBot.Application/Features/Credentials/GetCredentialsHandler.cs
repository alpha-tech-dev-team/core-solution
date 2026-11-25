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
    private const string PasswordMarkerPrefix = "__password_";
    private const string PasswordMarkerSuffix = "__";

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

        var passwordMarker = CreatePasswordMarker();
        var textWithMarker = string.Format(
            _localizer.GetPhrase(nameof(WorkflowStep.ServiceSelection), LocalizationKeysConstants.ServiceSelection.CredentialsTemplate),
            secret.ItemName,
            _localizer.GetPhrase(nameof(WorkflowStep.ServiceSelection), LocalizationKeysConstants.ServiceSelection.LoginLabel),
            secret.Login,
            _localizer.GetPhrase(nameof(WorkflowStep.ServiceSelection), LocalizationKeysConstants.ServiceSelection.PasswordLabel),
            passwordMarker);
        var passwordOffset = textWithMarker.IndexOf(passwordMarker, StringComparison.Ordinal);

        if (passwordOffset < 0)
        {
            throw new FormatException("Credentials template did not preserve the password marker");
        }

        var text = textWithMarker.Replace(passwordMarker, secret.Password, StringComparison.Ordinal);

        return Result<WorkflowResponse>.Success(new WorkflowResponse
        {
            Text = text,
            ContainsSensitiveData = true,
            TextMasks = CreateTextMasks(passwordOffset, secret.Password)
        });
    }

    private static string CreatePasswordMarker()
    {
        return $"{PasswordMarkerPrefix}{Guid.NewGuid():N}{PasswordMarkerSuffix}";
    }

    private static IReadOnlyList<TextMask>? CreateTextMasks(int passwordOffset, string? password)
    {
        if (string.IsNullOrEmpty(password))
        {
            return null;
        }

        return [new TextMask(passwordOffset, password.Length)];
    }
}
