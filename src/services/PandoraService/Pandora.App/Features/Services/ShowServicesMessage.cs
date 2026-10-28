using CoreSolution.Shared.Models;
using CoreSolution.Shared.Results;
using MediatR;

namespace Pandora.App.Features.Services;

public record ShowServicesMessage(long TelegramUserId, int MessageId) : IRequest<Result<TelegramMessageContent>>;
