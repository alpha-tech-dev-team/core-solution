using CoreSolution.Shared.Abstractions;
using CoreSolution.Shared.Models;
using CoreSolution.Shared.Results;
using MediatR;

namespace Pandora.Application.Features.Credentials;

public record GetCredentialsMessage(long TelegramUserId, int MessageId, string ItemId) : IRequest<Result<WorkflowResponse>>, IAuthorizedBotRequest;
