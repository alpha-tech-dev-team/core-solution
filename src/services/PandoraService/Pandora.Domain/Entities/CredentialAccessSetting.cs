namespace Pandora.Domain.Entities;

public sealed class CredentialAccessSetting
{
    public int Id { get; set; }
    public int TtlMinutes { get; set; }
    public DateTime UpdatedAt { get; set; }
    public Guid? UpdatedBy { get; set; }
}
