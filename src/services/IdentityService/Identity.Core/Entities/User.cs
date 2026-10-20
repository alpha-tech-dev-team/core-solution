using Identity.Core.Interfaces.Entities;

namespace Identity.Core.Entities;

public class User : IEntity
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Surname { get; set; } = string.Empty;
    public string Patronymic { get; set; } = string.Empty;
    public string Position { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string TelegramUserId { get; set; } = string.Empty;
    public string AdminUsername { get; set; } = string.Empty;
    public string AdminPasswordHash { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public Guid UpdatedById { get; set; }
    public User UpdatedBy { get; set; } = default!;
}