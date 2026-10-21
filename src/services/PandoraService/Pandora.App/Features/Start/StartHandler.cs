using MediatR;
using Pandora.Core.Interfaces;

namespace Pandora.App.Features.Start;

public sealed class StartHandler : IRequestHandler<StartMessage, string>
{
    private const string WelcomeText = "Pandora на связи. Бот работает.";
    private const string NoAccessText = "Для вас пока не открыт доступ ни к одному сервису. Обратитесь к администратору.";

    private readonly IAccessService _accessService;

    public StartHandler(IAccessService accessService)
    {
        _accessService = accessService;
    }

    public async Task<string> Handle(StartMessage request, CancellationToken cancellationToken)
    {
        var hasAccess = await _accessService.HasAccessAsync(request.ChatId, cancellationToken);

        return hasAccess ? WelcomeText : NoAccessText;
    }
}
