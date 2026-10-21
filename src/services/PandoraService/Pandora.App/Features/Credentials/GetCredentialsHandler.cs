using MediatR;
using Pandora.App.Common;
using Pandora.Core.Interfaces;

namespace Pandora.App.Features.Credentials;

public sealed class GetCredentialsHandler : IRequestHandler<GetCredentialsMessage, BotResponse>
{
    private const string NoAccessText = "У вас нет доступа к этому сервису.";
    private const string NotFoundText = "Учётные данные для этого сервиса не найдены. Обратитесь к администратору.";
    private const string LoginLabel = "Логин";
    private const string PasswordLabel = "Пароль";
    private readonly IAccessService _accessService;
    private readonly ISecretStore _secretStore;

    public GetCredentialsHandler(IAccessService accessService, ISecretStore secretStore)
    {
        _accessService = accessService;
        _secretStore = secretStore;
    }

    public async Task<BotResponse> Handle(GetCredentialsMessage request, CancellationToken cancellationToken)
    {
        var services = await _accessService.GetServices(request.ChatId, cancellationToken);

        if (!services.Contains(request.ServiceName))
        {
            return new BotResponse(NoAccessText, []);
        }

        var secret = await _secretStore.GetSecret(request.ServiceName, cancellationToken);

        if (secret is null)
        {
            return new BotResponse(NotFoundText, []);
        }

        var text = $"{request.ServiceName}\n{LoginLabel}: {secret.Login}\n{PasswordLabel}: {secret.Password}";

        return new BotResponse(text, []);
    }
}
