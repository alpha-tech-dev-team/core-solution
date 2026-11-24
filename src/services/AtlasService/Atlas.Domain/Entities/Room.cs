using Atlas.Domain.Interfaces;

namespace Atlas.Domain.Entities;

public sealed class Room : IEntity
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; }

    public ICollection<Hardware> Hardware { get; set; } = new List<Hardware>();
    public ICollection<Incident> Incidents { get; set; } = new List<Incident>();

    public Room()
    {
    }

    public Room(string name, string? description)
    {
        Name = name.Trim();
        Description = NormalizeDescription(description);
    }

    public void UpdateDetails(string name, string? description)
    {
        Name = name.Trim();
        Description = NormalizeDescription(description);
    }

    private static string? NormalizeDescription(string? description)
    {
        return string.IsNullOrWhiteSpace(description) ? null : description.Trim();
    }
}
