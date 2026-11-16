using CoreSolution.Shared.Models;
using CoreSolution.Shared.Results;
using MediatR;

namespace Pandora.TgBot.Application.Features.Services;

public record ShowServicesMessage(long TelegramUserId, int MessageId) : IRequest<Result<WorkflowResponse>>;
