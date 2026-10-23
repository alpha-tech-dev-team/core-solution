using Pandora.Core.Entities;

namespace Pandora.Core.Interfaces;

public interface IRequestContext
{
    UserSession Session { get; set; }
}
