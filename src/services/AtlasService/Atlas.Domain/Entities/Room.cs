namespace Atlas.Domain.Entities;

public sealed class Room
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; }

    public ICollection<Hardware> Hardware { get; set; } = new List<Hardware>();
    public ICollection<Incident> Incidents { get; set; } = new List<Incident>();
}
