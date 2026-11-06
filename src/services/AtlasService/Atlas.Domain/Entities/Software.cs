using Atlas.Domain.Interfaces;

namespace Atlas.Domain.Entities;

public sealed class Software : IHasUpdatedAt
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Version { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public ICollection<Incident> Incidents { get; set; } = new List<Incident>();
}
