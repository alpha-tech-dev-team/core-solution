namespace Identity.Api.Constants;

public static class ApiRoutes
{
    public const string ClientGroup = "/api/client";
    public const string ActivateUser = "activate";
    public const string CheckUser = "check";

    public const string AdminGroup = "/api/admin";
    public const string Login = "login";
    public const string RefreshToken = "refresh";
    public const string Logout = "logout";
    public const string Users = "users";
    public const string UserById = "users/{id:guid}";
    public const string Roles = "roles";
    public const string UserRoles = "users/{id:guid}/roles";
    public const string UserStatus = "users/{id:guid}/status";
}
