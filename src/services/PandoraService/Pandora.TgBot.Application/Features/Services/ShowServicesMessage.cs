using CoreSolution.Shared.Models;
using CoreSolution.Shared.Results;
using MediatR;
using Pandora.Domain.Constants;
using Pandora.TgBot.Application.Common.Interfaces;

namespace Pandora.TgBot.Application.Features.Services;

public record ShowServicesMessage(long TelegramUserId, int MessageId) : IRequest<Result<WorkflowResponse>>, IUserActionMessage
{
    public string Action => AuditConstants.Actions.ShowServices;
    public string? TargetItemId => null;
}
