namespace Pandora.Api.Constants;

public static class ApiRoutes
{
    public const string AdminGroup = "/api/admin";
    public const string Secrets = "secrets";
    public const string SecretById = "secrets/{itemId}";
    public const string EmployeeSecrets = "employees/{telegramUserId:long}/secrets";
    public const string EmployeeSecretById = "employees/{telegramUserId:long}/secrets/{itemId}";
    public const string CredentialAccessSettings = "settings/credential-access";
}
