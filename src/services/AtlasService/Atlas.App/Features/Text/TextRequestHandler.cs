using CoreSolution.Shared.Models;
using CoreSolution.Shared.Results;
using MediatR;

namespace Atlas.App.Features.Text;

public sealed class TextRequestHandler : IRequestHandler<TextRequest, Result<TelegramMessageContent>>
{
    public Task<Result<TelegramMessageContent>> Handle(TextRequest request, CancellationToken ctn)
    {
        var content = new TelegramMessageContent
        {
          Text=$"сообщение {request.Text} от пользователя {request.Name}"
        };
        
        return Task.FromResult(Result<TelegramMessageContent>.Success(content));
    }
}
