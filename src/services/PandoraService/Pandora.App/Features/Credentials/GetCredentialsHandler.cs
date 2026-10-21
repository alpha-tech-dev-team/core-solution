using CoreSolution.Shared.Models;
using CoreSolution.Shared.Results;
using MediatR;
using Pandora.Core.Enums;
using Pandora.Core.Interfaces;

namespace Pandora.App.Features.Credentials;

public sealed class GetCredentialsHandler : IRequestHandler<GetCredentialsMessage, Result<TelegramMessageContent>>
{
    private readonly IAccessService _accessService;
    private readonly ISecretStore _secretStore;

    public GetCredentialsHandler(IAccessService accessService, ISecretStore secretStore)
    {
        _accessService = accessService;
        _secretStore = secretStore;
    }

    public async Task<Result<TelegramMessageContent>> Handle(GetCredentialsMessage request, CancellationToken cancellationToken)
    {
        var services = await _accessService.GetServices(request.ChatId, cancellationToken);

        if (!services.Contains(request.ServiceName))
        {
            return Result.Failure<TelegramMessageContent>(ErrorCode.AccessDenied.ToString(), [GetCredentialsConstants.NoAccessText]);
        }

        var secret = await _secretStore.GetSecret(request.ServiceName, cancellationToken);

        if (secret is null)
        {
            return Result.Failure<TelegramMessageContent>(ErrorCode.SecretNotFound.ToString(), [GetCredentialsConstants.NotFoundText]);
        }

        var text = string.Format(
            GetCredentialsConstants.CredentialsTemplate,
            request.ServiceName,
            GetCredentialsConstants.LoginLabel,
            secret.Login,
            GetCredentialsConstants.PasswordLabel,
            secret.Password);

        return Result<TelegramMessageContent>.Success(new TelegramMessageContent { Text = text });
    }
}
