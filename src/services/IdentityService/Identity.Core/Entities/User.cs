using Identity.Core.Enums;
using Identity.Core.Interfaces.Entities;

namespace Identity.Core.Entities;

public class User : IEntity
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Surname { get; set; } = string.Empty;
    public string? Patronymic { get; set; }
    public string Position { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public long? TelegramUserId { get; set; }
    public UserStatus Status { get; set; } = UserStatus.Pending;
    public string? AdminUsername { get; set; }
    public string? AdminPasswordHash { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public Guid UpdatedById { get; set; }
    public User UpdatedBy { get; set; } = default!;

    public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
}