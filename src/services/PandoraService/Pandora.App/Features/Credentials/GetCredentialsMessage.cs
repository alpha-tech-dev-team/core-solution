using Building.Telegram.Models;
using Building.Telegram.Results;
using MediatR;

namespace Pandora.App.Features.Credentials;

public record GetCredentialsMessage(long ChatId, string ServiceName) : IRequest<Result<TelegramMessageContent>>;
