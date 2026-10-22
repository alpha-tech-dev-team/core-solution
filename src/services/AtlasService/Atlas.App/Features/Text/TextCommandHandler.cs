using CoreSolution.Shared.Models;
using CoreSolution.Shared.Results;
using MediatR;

namespace Atlas.App.Features.Text;

public sealed class TextCommandHandler : IRequestHandler<TextCommand, Result<TelegramMessageContent>>
{
    public Task<Result<TelegramMessageContent>> Handle(TextCommand request, CancellationToken cancellationToken)
    {
        var content = new TelegramMessageContent
        {
          Text=$"сообщение {request.Text} от пользователя {request.Name}"
        };
        
        return Task.FromResult(Result<TelegramMessageContent>.Success(content));
    }
}
