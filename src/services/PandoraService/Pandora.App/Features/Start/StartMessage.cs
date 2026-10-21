using MediatR;
using Pandora.App.Common;

namespace Pandora.App.Features.Start;

public record StartMessage(long ChatId) : IRequest<BotResponse>;
