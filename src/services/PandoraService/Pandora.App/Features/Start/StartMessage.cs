using MediatR;

namespace Pandora.App.Features.Start;

public record StartMessage(long ChatId) : IRequest<string>;
