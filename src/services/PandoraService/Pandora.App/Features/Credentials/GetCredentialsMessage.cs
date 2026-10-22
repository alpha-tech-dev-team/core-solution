using CoreSolution.Shared.Models;
using CoreSolution.Shared.Results;
using MediatR;
using Pandora.Core.Interfaces;

namespace Pandora.App.Features.Credentials;

public record GetCredentialsMessage(long ChatId, string ServiceName) : IRequest<Result<TelegramMessageContent>>, IAuthorizedBotRequest;
