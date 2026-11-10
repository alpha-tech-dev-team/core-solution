using CoreSolution.Shared.Abstractions;
using CoreSolution.Shared.Models;
using CoreSolution.Shared.Results;
using MediatR;
using Pandora.Application.Common.Interfaces;
using Pandora.Domain.Constants;

namespace Pandora.Application.Features.Credentials;

public record GetCredentialsMessage(long TelegramUserId, int MessageId, string ItemId) : IRequest<Result<WorkflowResponse>>, IAuthorizedBotRequest, IUserActionMessage
{
    public string Action => AuditConstants.ActionGetCredentials;
    public string? TargetItemId => ItemId;
}
