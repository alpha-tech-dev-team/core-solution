namespace Atlas.Domain.Entities;

public sealed class Software
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Version { get; set; }

    public ICollection<Incident> Incidents { get; set; } = new List<Incident>();
}
