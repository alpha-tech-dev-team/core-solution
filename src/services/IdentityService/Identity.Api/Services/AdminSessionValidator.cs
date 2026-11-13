using System.Security.Claims;
using Identity.Application.Errors;
using Identity.Domain.Constants;
using Identity.Domain.Enums;
using Identity.Domain.Interfaces.Repositories;
using UserEntity = Identity.Domain.Entities.User;

namespace Identity.Api.Services;

public sealed class AdminSessionValidator
{
    private readonly IUserRepository _users;

    public AdminSessionValidator(IUserRepository users)
    {
        _users = users;
    }

    public async Task<string?> ValidateAsync(ClaimsPrincipal principal, CancellationToken cancellationToken)
    {
        var subject = principal.FindFirst(IdentityClaims.Subject)?.Value;

        if (!Guid.TryParse(subject, out var userId))
        {
            return nameof(IdentityErrorCode.InvalidCredentials);
        }

        var user = await _users.GetById(userId, includeRoles: true, cancellationToken);

        if (user is null)
        {
            return nameof(IdentityErrorCode.InvalidCredentials);
        }

        if (user.Status == UserStatus.Blocked)
        {
            return nameof(IdentityErrorCode.UserBlocked);
        }

        if (user.Status == UserStatus.Archived)
        {
            return nameof(IdentityErrorCode.AccessDenied);
        }

        SyncClaims(principal, user);

        return null;
    }

    private static void SyncClaims(ClaimsPrincipal principal, UserEntity user)
    {
        if (principal.Identity is not ClaimsIdentity identity)
        {
            return;
        }

        foreach (var claim in identity.FindAll(IdentityClaims.Role).ToList())
        {
            identity.RemoveClaim(claim);
        }

        foreach (var role in user.UserRoles.Select(x => x.Role.Name).Distinct(StringComparer.OrdinalIgnoreCase))
        {
            identity.AddClaim(new Claim(IdentityClaims.Role, role));
        }

        var login = identity.FindFirst(IdentityClaims.UniqueName);
        if (login is not null)
        {
            identity.RemoveClaim(login);
        }

        if (user.AdminUsername is not null)
        {
            identity.AddClaim(new Claim(IdentityClaims.UniqueName, user.AdminUsername));
        }
    }
}
