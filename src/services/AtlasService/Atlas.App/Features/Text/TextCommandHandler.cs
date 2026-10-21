using MediatR;

namespace Atlas.App.Features.Text;

public sealed class TextCommandHandler : IRequestHandler<TextCommand, string>
{
    public Task<string> Handle(TextCommand request, CancellationToken cancellationToken)
    {
        return Task.FromResult($"сообщение {request.Text} от пользователя {request.Name}");
    }
}
