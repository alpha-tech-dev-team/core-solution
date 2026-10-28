using CoreSolution.Shared.Models;
using CoreSolution.Shared.Results;
using MediatR;

namespace Atlas.App.Features.Activation;

public record SubmitPhoneMessage(long TelegramUserId, int MessageId, string Phone) : IRequest<Result<TelegramMessageContent>>;
