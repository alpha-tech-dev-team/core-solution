using CoreSolution.Shared.Models;
using CoreSolution.Shared.Results;
using MediatR;

namespace CoreSolution.Shared.Application.Messages;

public record SubmitPhoneMessage(long TelegramUserId, int MessageId, string? Phone) : IRequest<Result<WorkflowResponse>>;
