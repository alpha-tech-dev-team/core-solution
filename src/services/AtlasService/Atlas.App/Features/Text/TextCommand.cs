using MediatR;

namespace Atlas.App.Features.Text;

public record TextCommand(long ChatId, string Text, string Name) : IRequest<string>;
