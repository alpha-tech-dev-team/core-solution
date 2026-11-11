namespace CoreSolution.Shared.Enums;

public static class UserRoleExtensions
{
    private const string AdminRoleName = "Admin";

    public static string ToRoleName(this UserRole role)
    {
        return role switch
        {
            UserRole.AtlasAdmin or UserRole.PandoraAdmin => AdminRoleName,
            _ => role.ToString()
        };
    }
}
