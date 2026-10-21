using Building.Telegram.Models;
using Building.Telegram.Results;
using MediatR;

namespace Pandora.App.Features.Start;

public record StartMessage(long ChatId) : IRequest<Result<TelegramMessageContent>>;
