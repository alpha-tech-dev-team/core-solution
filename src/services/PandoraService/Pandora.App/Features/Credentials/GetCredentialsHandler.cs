using MediatR;
using Pandora.Core.Interfaces;

namespace Pandora.App.Features.Credentials;

public sealed class GetCredentialsHandler : IRequestHandler<GetCredentialsMessage, string>
{
    private const string NoAccessText = "У вас нет доступа к этому сервису.";
    private const string NotFoundText = "Учётные данные для этого сервиса не найдены. Обратитесь к администратору.";

    private readonly IAccessService _accessService;
    private readonly ISecretStore _secretStore;

    public GetCredentialsHandler(IAccessService accessService, ISecretStore secretStore)
    {
        _accessService = accessService;
        _secretStore = secretStore;
    }

    public async Task<string> Handle(GetCredentialsMessage request, CancellationToken cancellationToken)
    {
        var services = await _accessService.GetServicesAsync(request.ChatId, cancellationToken);

        if (services is null || !services.Contains(request.ServiceName))
        {
            return NoAccessText;
        }

        var secret = await _secretStore.GetSecretAsync(request.ServiceName, cancellationToken);

        if (secret is null)
        {
            return NotFoundText;
        }

        return $"{request.ServiceName}\nЛогин: {secret.Login}\nПароль: {secret.Password}";
    }
}
