using MediatR;
using Pandora.App.Common;

namespace Pandora.App.Features.Credentials;

public record GetCredentialsMessage(long ChatId, string ServiceName) : IRequest<BotResponse>;
