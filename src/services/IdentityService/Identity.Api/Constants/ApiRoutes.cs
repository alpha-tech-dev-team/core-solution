using CoreSolution.Shared.Constants;

namespace Identity.Api.Constants;

public static class ApiRoutes
{
    public const string ClientGroup = ClientApiRoutes.Group;
    public const string ActivateUser = ClientApiRoutes.Activate;
    public const string CheckUser = ClientApiRoutes.Check;
    public const string Employee = ClientApiRoutes.EmployeeById;

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
