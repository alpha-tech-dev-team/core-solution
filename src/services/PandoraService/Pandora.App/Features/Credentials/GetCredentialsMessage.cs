using CoreSolution.Shared.Models;
using CoreSolution.Shared.Results;
using MediatR;
using Pandora.Core.Interfaces;

namespace Pandora.App.Features.Credentials;

public record GetCredentialsMessage(long TelegramUserId, string ServiceName) : IRequest<Result<TelegramMessageContent>>, IAuthorizedBotRequest;
