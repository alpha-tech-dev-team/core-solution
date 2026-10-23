using MediatR;
using CoreSolution.Shared.Models;
using CoreSolution.Shared.Results;

namespace Atlas.App.Features.Start;

public record StartRequest(long ChatId) : IRequest<Result<TelegramMessageContent>>;
