using Atlas.Core.Entities;
using Atlas.Core.Interfaces;

namespace Atlas.Infrastructure.Utils;

public class RequestContext : IRequestContext
{
    public UserSession? Session { get; set; }
}
