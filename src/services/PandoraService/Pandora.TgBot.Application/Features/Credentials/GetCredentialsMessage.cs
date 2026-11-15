using CoreSolution.Shared.Abstractions;
using CoreSolution.Shared.Models;
using CoreSolution.Shared.Results;
using MediatR;
using Pandora.Domain.Constants;
using Pandora.TgBot.Application.Common.Interfaces;

namespace Pandora.TgBot.Application.Features.Credentials;

public record GetCredentialsMessage(long TelegramUserId, int MessageId, string ItemId) : IRequest<Result<WorkflowResponse>>, IAuthorizedBotRequest, IUserActionMessage
{
    public string Action => AuditConstants.Actions.GetCredentials;
    public string? TargetItemId => ItemId;
}
