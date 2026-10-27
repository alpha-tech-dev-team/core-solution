namespace Pandora.Infrastructure.Database;

public static class DatabaseConstants
{
    public static class SecretMetadata
    {
        public const string Table = "secret_metadata";
        public const string VaultItemId = "vault_item_id";
        public const string ResourceName = "resource_name";
        public const string Username = "username";
        public const int VaultItemIdMaxLength = 36;
        public const int ResourceNameMaxLength = 100;
        public const int UsernameMaxLength = 100;
    }

    public static class Permissions
    {
        public const string Table = "permissions";
        public const string UserId = "user_id";
        public const string VaultItemId = "vault_item_id";
        public const string IsActive = "is_active";
        public const int VaultItemIdMaxLength = 36;
        public const bool IsActiveDefault = true;
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
        public const int ActionMaxLength = 50;
        public const int TargetItemIdMaxLength = 36;
        public const int RequestSourceMaxLength = 20;
    }
}
