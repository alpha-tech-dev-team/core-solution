using MediatR;

namespace Pandora.App.Features.Start;

public sealed class StartHandler : IRequestHandler<StartMessage, string>
{
    private const string Response = "Pandora на связи. Бот работает.";

    public Task<string> Handle(StartMessage request, CancellationToken cancellationToken)
    {
        return Task.FromResult(Response);
    }
}
