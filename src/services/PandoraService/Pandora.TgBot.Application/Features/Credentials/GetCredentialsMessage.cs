using CoreSolution.Shared.Abstractions;
using CoreSolution.Shared.Models;
using CoreSolution.Shared.Results;
using MediatR;
using Pandora.TgBot.Application.Common.Interfaces;
using Pandora.Domain.Constants;

namespace Pandora.TgBot.Application.Features.Credentials;

public record GetCredentialsMessage(long TelegramUserId, int MessageId, string ItemId) : IRequest<Result<WorkflowResponse>>, IAuthorizedBotRequest, IUserActionMessage
{
    public string Action => AuditConstants.Actions.GetCredentials;
    public string? TargetItemId => ItemId;
}
