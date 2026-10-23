using CoreSolution.Shared.Models;
using CoreSolution.Shared.Results;
using MediatR;

namespace Pandora.App.Features.Start;

public record StartMessage(long TelegramUserId, int MessageId) : IRequest<Result<TelegramMessageContent>>;
