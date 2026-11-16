namespace Pandora.Domain.Constants;

public static class AuditConstants
{
    public const string ActionSecretCreated = "secret_created";
    public const string ActionSecretUpdated = "secret_updated";
    public const string ActionSecretDeleted = "secret_deleted";
    public const string ActionSecretAccessGranted = "secret_access_granted";
    public const string ActionSecretAccessRevoked = "secret_access_revoked";
    public const string ActionSecretPasswordsViewed = "secret_passwords_viewed";
    public const string SourceWeb = "web";
}
