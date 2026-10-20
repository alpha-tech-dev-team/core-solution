using Identity.Core.Interfaces.Entities;

namespace Identity.Core.Entities;

public class Roles : IEntity
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;

    public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
}