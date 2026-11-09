using Microsoft.AspNetCore.Authorization;
using Pandora.Api.Constants;

namespace Pandora.Api.Authorization;

public sealed class AdminUserIdAuthorizationHandler : AuthorizationHandler<AdminUserIdRequirement>
{
    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, AdminUserIdRequirement requirement)
    {
        var subject = context.User.FindFirst(PandoraJwtConstants.SubjectClaim)?.Value;

        if (Guid.TryParse(subject, out _))
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}
