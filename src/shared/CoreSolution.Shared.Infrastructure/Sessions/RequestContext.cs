using CoreSolution.Shared.Abstractions;
using CoreSolution.Shared.Models;

namespace CoreSolution.Shared.Infrastructure.Sessions;

public sealed class RequestContext : IRequestContext
{
    public UserSession Session { get; set; } = default!;
}
