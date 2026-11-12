using CoreSolution.Shared.Enums;

namespace Identity.Domain.Models;

public sealed record UserRolesData(UserRole[] Roles, string? AdminLogin, string? AdminPassword);
