using Pandora.Domain.Constants;

namespace Pandora.Infrastructure.Database;

public static class DatabaseConstants
{
    public static class SecretMetadata
    {
        public const string Table = "secret_metadata";
        public const string VaultItemId = "vault_item_id";
        public const string ResourceName = "resource_name";
        public const string Username = "username";
        public const string SyncOperation = "sync_operation";
        public const string AttemptCount = "attempt_count";
        public const string NextAttemptAt = "next_attempt_at";
        public const string LastError = "last_error";
        public const int VaultItemIdMaxLength = SecretMetadataLimits.VaultItemIdMaxLength;
        public const int ResourceNameMaxLength = 100;
        public const int UsernameMaxLength = 100;
        public const int SyncOperationMaxLength = 20;
        public const int LastErrorMaxLength = 500;
    }

    public static class Permissions
    {
        public const string Table = "permissions";
        public const string TelegramUserId = "telegram_user_id";
        public const string VaultItemId = "vault_item_id";
        public const string IsActive = "is_active";
        public const string GrantedAt = "granted_at";
        public const string GrantedBy = "granted_by";
        public const int VaultItemIdMaxLength = SecretMetadataLimits.VaultItemIdMaxLength;
        public const bool IsActiveDefault = true;
        public const bool IsActiveGranted = true;
        public const bool IsActiveRevoked = false;
    }

    public static class Audit
    {
        public const string Table = "audit";
        public const string Id = "id";
        public const string Timestamp = "timestamp";
        public const string UserId = "user_id";
        public const string TelegramUserId = "telegram_user_id";
        public const string Action = "action";
        public const string TargetItemId = "target_item_id";
        public const string RequestSource = "request_source";
        public const string TargetTelegramUserId = "target_telegram_user_id";
        public const int ActionMaxLength = 50;
        public const int TargetItemIdMaxLength = 36;
        public const int RequestSourceMaxLength = 20;
    }

    public static class AuditOutbox
    {
        public const string Table = "audit_outbox";
        public const string Id = "id";
        public const string Payload = "payload";
        public const string CreatedAt = "created_at";
    }
}
