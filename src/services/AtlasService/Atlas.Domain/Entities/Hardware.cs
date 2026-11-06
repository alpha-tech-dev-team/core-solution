using Atlas.Domain.Interfaces;

namespace Atlas.Domain.Entities;

public sealed class Hardware : IHasUpdatedAt
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string? InventoryNumber { get; set; }
    public string? SerialNumber { get; set; }
    public Guid CabinetId { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public Cabinet? Cabinet { get; set; }
    public ICollection<Incident> Incidents { get; set; } = new List<Incident>();
}
