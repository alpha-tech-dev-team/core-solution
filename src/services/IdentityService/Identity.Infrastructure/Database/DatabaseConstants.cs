namespace Identity.Infrastructure.Database;

public static class DatabaseConstants
{
    public const string TimestampWithTimeZone = "timestamp with time zone";
    public const string CurrentTimestampSql = "now()";

    public static class Tables
    {
        public const string Users = "users";
        public const string Roles = "roles";
        public const string UserRoles = "user_roles";
    }

    public static class Columns
    {
        public const string Id = "id";
        public const string Name = "name";
        public const string Surname = "surname";
        public const string Patronymic = "patronymic";
        public const string Position = "position";
        public const string PhoneNumber = "phone_number";
        public const string TelegramUserId = "telegram_user_id";
        public const string Status = "status";
        public const string AdminLogin = "admin_login";
        public const string AdminPasswordHash = "admin_password_hash";
        public const string CreatedAt = "created_at";
        public const string UpdatedAt = "updated_at";
        public const string UpdatedById = "updated_by_id";
        public const string UserId = "user_id";
        public const string RoleId = "role_id";
    }
}
