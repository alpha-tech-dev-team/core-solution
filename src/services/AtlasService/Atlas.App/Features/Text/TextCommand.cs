using MediatR;
using CoreSolution.Shared.Models;
using CoreSolution.Shared.Results;

namespace Atlas.App.Features.Text;

public record TextCommand(long ChatId, string Text, string Name) : IRequest<Result<TelegramMessageContent>>;
