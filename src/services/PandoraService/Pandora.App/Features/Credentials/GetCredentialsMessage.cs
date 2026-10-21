using MediatR;

namespace Pandora.App.Features.Credentials;

public record GetCredentialsMessage(long ChatId, string ServiceName) : IRequest<string>;
