using Identity.App.Errors;

namespace Identity.Api.Constants;

internal static class IdentityErrorStatusCodes
{
    private static readonly IReadOnlyDictionary<IdentityErrorCode, int> StatusCodesByError =
        new Dictionary<IdentityErrorCode, int>
        {
            [IdentityErrorCode.UserNotFound] = StatusCodes.Status404NotFound,
            [IdentityErrorCode.UserAlreadyActivated] = StatusCodes.Status409Conflict,
            [IdentityErrorCode.AccessDenied] = StatusCodes.Status403Forbidden,
            [IdentityErrorCode.InvalidCredentials] = StatusCodes.Status401Unauthorized,
            [IdentityErrorCode.InvalidRefreshToken] = StatusCodes.Status401Unauthorized,
            [IdentityErrorCode.InternalError] = StatusCodes.Status500InternalServerError
        };

    public static int Get(string? errorCode)
    {
        return Enum.TryParse<IdentityErrorCode>(errorCode, out var code)
            && StatusCodesByError.TryGetValue(code, out var statusCode)
                ? statusCode
                : StatusCodesByError[IdentityErrorCode.InternalError];
    }
}
