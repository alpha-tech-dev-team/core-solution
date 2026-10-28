using Atlas.Core.Entities;

namespace Atlas.Core.Interfaces;

public interface IRequestContext
{
    UserSession Session { get; set; }
}
