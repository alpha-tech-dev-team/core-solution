using CoreSolution.Shared.Enums;

namespace Identity.Application.Contracts.Admin;

public record SetUserRolesRequest(
    UserRole[] Roles,
    string? AdminLogin = null,
    string? AdminPassword = null);
