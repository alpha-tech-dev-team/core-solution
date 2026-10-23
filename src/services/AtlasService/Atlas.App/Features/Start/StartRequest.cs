using CoreSolution.Shared.Models;
using CoreSolution.Shared.Results;
using MediatR;

namespace Atlas.App.Features.Start;

public record StartRequest(long TgUserId) : IRequest<Result<TelegramMessageContent>>;