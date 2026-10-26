namespace Pandora.Infrastructure.Database;

public static class DatabaseConstants
{
    public static class SecretMetadata
    {
        public const string Table = "secret_metadata";
        public const string VaultItemId = "vault_item_id";
        public const string ResourceName = "resource_name";
        public const string Username = "username";
    }

    public static class Permissions
    {
        public const string Table = "permissions";
        public const string UserId = "user_id";
        public const string VaultItemId = "vault_item_id";
        public const string IsActive = "is_active";
    }

    public static class Audit
    {
        public const string Table = "audit";
        public const string Id = "id";
        public const string Timestamp = "timestamp";
        public const string UserId = "user_id";
        public const string Action = "action";
        public const string TargetItemId = "target_item_id";
        public const string RequestSource = "request_source";
    }
}
