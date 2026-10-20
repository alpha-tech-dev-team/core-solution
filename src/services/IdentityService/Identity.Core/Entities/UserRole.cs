namespace Identity.Core.Entities;

public class UserRole
{
    public Guid UserId { get; set; }
    public User User { get; set; } = default!;

    public Guid RoleId { get; set; }
    public Roles Role { get; set; } = default!;
}