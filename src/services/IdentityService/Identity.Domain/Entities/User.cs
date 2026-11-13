using Identity.Domain.Enums;
using Identity.Domain.Interfaces.Entities;
using Identity.Domain.Models;

namespace Identity.Domain.Entities;

public class User : IEntity, IAuditableEntity
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Surname { get; set; } = string.Empty;
    public string? Patronymic { get; set; }
    public string Position { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public long? TelegramUserId { get; set; }
    public UserStatus Status { get; set; } = UserStatus.Pending;
    public bool IsDeleted { get; set; }
    public string? AdminUsername { get; set; }
    public string? AdminPasswordHash { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public Guid UpdatedById { get; set; }
    public User UpdatedBy { get; set; } = default!;

    public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();

    public User()
    {
    }

    public User(EmployeeData data)
    {
        Id = Guid.NewGuid();
        Name = data.Name;
        Surname = data.Surname;
        Patronymic = data.Patronymic;
        Position = data.Position;
        Phone = data.Phone;
        Status = UserStatus.Pending;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
        UpdatedById = Id;
    }
}
