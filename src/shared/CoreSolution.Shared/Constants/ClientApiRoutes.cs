namespace CoreSolution.Shared.Constants;

public static class ClientApiRoutes
{
    public const string Group = "/api/client";
    public const string ServiceApiKeyHeader = "X-Internal-Api-Key";

    public const string Activate = "activate";
    public const string Check = "check";
    public const string EmployeeById = "employee/{telegramUserId:long}";

    public static string EmployeeByTelegramUser(long telegramUserId)
    {
        return $"{Group}/employee/{telegramUserId}";
    }
}
