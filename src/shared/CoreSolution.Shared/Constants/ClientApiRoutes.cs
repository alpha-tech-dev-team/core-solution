namespace CoreSolution.Shared.Constants;

public static class ClientApiRoutes
{
    public const string Group = "/api/client";
    public const string ServiceApiKeyHeader = "X-Internal-Api-Key";
    public const string TelegramUserIdRouteValue = "telegramUserId";

    public const string Activate = "activate";
    public const string Check = "check";
    public const string Employee = "employee";
    public const string EmployeeById = $"{Employee}/{{{TelegramUserIdRouteValue}:long}}";

    private const string EmployeeRouteFormat = "{0}/{1}/{2}";

    public static string EmployeeByTelegramUser(long telegramUserId)
    {
        return string.Format(EmployeeRouteFormat, Group, Employee, telegramUserId);
    }
}
