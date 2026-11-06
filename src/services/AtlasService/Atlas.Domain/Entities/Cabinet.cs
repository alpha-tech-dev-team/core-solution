using Atlas.Domain.Interfaces;

namespace Atlas.Domain.Entities;

public sealed class Cabinet : IHasUpdatedAt
{
    public Guid Id { get; set; }
    public string Number { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public ICollection<Hardware> Hardware { get; set; } = new List<Hardware>();
    public ICollection<Incident> Incidents { get; set; } = new List<Incident>();
}
