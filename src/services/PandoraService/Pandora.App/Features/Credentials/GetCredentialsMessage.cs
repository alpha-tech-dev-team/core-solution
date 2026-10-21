using CoreSolution.Shared.Models;
using CoreSolution.Shared.Results;
using MediatR;

namespace Pandora.App.Features.Credentials;

public record GetCredentialsMessage(long ChatId, string ServiceName) : IRequest<Result<TelegramMessageContent>>;
