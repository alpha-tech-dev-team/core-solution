namespace Identity.Domain.Interfaces.Entities;

public interface IAuditableEntity
{
    DateTime UpdatedAt { get; set; }
}
