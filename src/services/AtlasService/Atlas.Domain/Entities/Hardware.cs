using Atlas.Domain.Interfaces;

namespace Atlas.Domain.Entities;

public sealed class Hardware : IHasUpdatedAt
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string? InventoryNumber { get; set; }
    public string? SerialNumber { get; set; }
    public Guid RoomId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public Room Room { get; set; } = default!;
    public ICollection<Incident> Incidents { get; set; } = new List<Incident>();
}
