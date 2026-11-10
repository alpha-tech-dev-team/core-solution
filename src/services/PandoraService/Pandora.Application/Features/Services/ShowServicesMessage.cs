using CoreSolution.Shared.Abstractions;
using CoreSolution.Shared.Models;
using CoreSolution.Shared.Results;
using MediatR;
using Pandora.Application.Common.Interfaces;
using Pandora.Domain.Constants;

namespace Pandora.Application.Features.Services;

public record ShowServicesMessage(long TelegramUserId, int MessageId) : IRequest<Result<WorkflowResponse>>, IUserActionMessage
{
    public string Action => AuditConstants.Actions.ShowServices;
    public string? TargetItemId => null;
}
