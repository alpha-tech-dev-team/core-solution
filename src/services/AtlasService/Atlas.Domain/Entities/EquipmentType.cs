using Atlas.Domain.Interfaces;

namespace Atlas.Domain.Entities;

public sealed class EquipmentType : IEntity
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }

    public ICollection<Hardware> Hardware { get; set; } = new List<Hardware>();
}
