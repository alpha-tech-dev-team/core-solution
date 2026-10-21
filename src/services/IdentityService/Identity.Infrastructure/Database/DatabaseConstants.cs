namespace Identity.Infrastructure.Database;

public static class DatabaseConstants
{
    public const string TimestampWithTimeZone = "timestamp with time zone";
    public const string CurrentTimestampSql = "now()";

    public static class Users
    {
        public const string Table = "users";
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

        public const int NameMaxLength = 50;
        public const int SurnameMaxLength = 50;
        public const int PatronymicMaxLength = 50;
        public const int PositionMaxLength = 50;
        public const int PhoneMaxLength = 20;
        public const int StatusMaxLength = 20;
        public const int AdminLoginMaxLength = 50;
    }

    public static class Roles
    {
        public const string Table = "roles";
        public const string Id = "id";
        public const string Name = "name";

        public const int NameMaxLength = 50;
    }

    public static class UserRoles
    {
        public const string Table = "user_roles";
        public const string UserId = "user_id";
        public const string RoleId = "role_id";
    }
}
