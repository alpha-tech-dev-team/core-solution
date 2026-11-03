using CoreSolution.Shared.Models;
using CoreSolution.Shared.Results;
using MediatR;
using CoreSolution.Shared.Abstractions;

namespace Pandora.Application.Features.Credentials;

public record GetCredentialsMessage(long TelegramUserId, int MessageId, string ServiceName) : IRequest<Result<WorkflowResponse>>, IAuthorizedBotRequest;
