using MediatR;

namespace Atlas.Core.Commands;

public record TextCommand(long ChatId, string Text, string Name) : IRequest<string>;

public class TextCommandHandler : IRequestHandler<TextCommand, string>
{
    public async Task<string> Handle(TextCommand request, CancellationToken ctn)
    {
        return $"сообщение {request.Text} от пользователя {request.Name}";
    }
}