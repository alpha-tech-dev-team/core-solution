namespace Pandora.Domain.Constants;

public static class AuditConstants
{
    public const string ActionSecretCreated = "secret_created";
    public const string ActionSecretUpdated = "secret_updated";
    public const string ActionSecretDeleted = "secret_deleted";
    public const string SourceWeb = "web";

    public const string AuditChannel = "audit:new";

    public static class Actions
    {
        public const string SubmitPhone = "submit_phone";
        public const string ShowServices = "show_services";
        public const string GetCredentials = "get_credentials";
    }

    public static class Sources
    {
        public const string Telegram = "telegram";
    }
}
