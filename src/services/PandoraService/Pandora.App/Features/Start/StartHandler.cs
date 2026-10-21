using MediatR;
using Pandora.Core.Interfaces;

namespace Pandora.App.Features.Start;

public sealed class StartHandler : IRequestHandler<StartMessage, StartResponse>
{
    private const string ServicesText = "Выберите сервис:";
    private const string NoServicesText = "Вам пока ничего не назначено. Обратитесь к администратору.";
    private const string NoAccessText = "Для вас пока не открыт доступ ни к одному сервису. Обратитесь к администратору.";

    private readonly IAccessService _accessService;

    public StartHandler(IAccessService accessService)
    {
        _accessService = accessService;
    }

    public async Task<StartResponse> Handle(StartMessage request, CancellationToken cancellationToken)
    {
        var services = await _accessService.GetServicesAsync(request.ChatId, cancellationToken);

        if (services is null)
        {
            return new StartResponse(NoAccessText, []);
        }

        if (services.Count == 0)
        {
            return new StartResponse(NoServicesText, []);
        }

        return new StartResponse(ServicesText, services);
    }
}
