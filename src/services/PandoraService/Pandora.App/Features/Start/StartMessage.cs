using CoreSolution.Shared.Models;
using CoreSolution.Shared.Results;
using MediatR;
using Pandora.Core.Interfaces;

namespace Pandora.App.Features.Start;

public record StartMessage(long TelegramUserId) : IRequest<Result<TelegramMessageContent>>, IAuthorizedBotRequest;
