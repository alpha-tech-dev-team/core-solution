using CoreSolution.Shared.Models;

namespace CoreSolution.Shared.Abstractions;

public interface IRequestContext
{
    UserSession Session { get; set; }
}
