using Pandora.Core.Entities;
using Pandora.Core.Interfaces;

namespace Pandora.Infrastructure.Utils;

public sealed class RequestContext : IRequestContext
{
    public UserSession Session { get; set; } = default!;
}
