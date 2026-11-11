using Identity.Api.Interfaces;
using Identity.Domain.Constants;

namespace Identity.Api.Services;

public sealed class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public Guid UserId
    {
        get
        {
            var subject = _httpContextAccessor.HttpContext?.User.FindFirst(IdentityClaims.Subject)?.Value;

            return Guid.TryParse(subject, out var userId)
                ? userId
                : throw new InvalidOperationException("В токене отсутствует идентификатор пользователя");
        }
    }

    public bool IsCurrentUser(Guid userId)
    {
        return UserId == userId;
    }
}
