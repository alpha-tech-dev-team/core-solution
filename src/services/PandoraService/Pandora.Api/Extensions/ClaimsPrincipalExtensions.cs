using System.Security.Claims;
using CoreSolution.Shared.Constants;

namespace Pandora.Api.Extensions;

public static class ClaimsPrincipalExtensions
{
    public static Guid GetAdminUserId(this ClaimsPrincipal principal)
    {
        var subject = principal.FindFirst(JwtClaimConstants.Subject)?.Value;

        return Guid.Parse(subject!);
    }
}
