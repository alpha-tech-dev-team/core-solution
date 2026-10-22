using Pandora.Core.Enums;

namespace Pandora.Core.Entities;

public class UserSession
{
    public required long Id { get; set; }

    public WorkflowStep State { get; set; }

    public DateTime LastUpdate { get; set; }
}
