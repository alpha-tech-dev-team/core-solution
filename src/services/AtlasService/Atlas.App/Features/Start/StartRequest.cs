using Atlas.Core.Enums;
using MediatR;

namespace Atlas.App.Features.Start;

public record StartRequest(long TgUserId) : IRequest<Result<TelegramMessageContent>>;