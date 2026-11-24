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

    public static string EmployeeByTelegramUser(long telegramUserId)
    {
        return $"{Group}/{Employee}/{telegramUserId}";
    }
}
