using System.Security.Claims;
using Pandora.Api.Constants;

namespace Pandora.Api.Extensions;

public static class ClaimsPrincipalExtensions
{
    public static Guid GetAdminUserId(this ClaimsPrincipal principal)
    {
        var subject = principal.FindFirst(PandoraJwtConstants.SubjectClaim)?.Value;

        return Guid.Parse(subject!);
    }
}
