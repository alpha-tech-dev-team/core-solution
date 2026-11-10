using Pandora.Domain.Enums;

namespace Pandora.Api.Constants;

internal static class PandoraErrorStatusCodes
{
    private static readonly IReadOnlyDictionary<ErrorCode, int> StatusCodesByError =
        new Dictionary<ErrorCode, int>
        {
            [ErrorCode.AccessDenied] = StatusCodes.Status403Forbidden,
            [ErrorCode.AccessNotConfirmed] = StatusCodes.Status403Forbidden,
            [ErrorCode.SecretNotFound] = StatusCodes.Status404NotFound,
            [ErrorCode.VaultwardenUnavailable] = StatusCodes.Status503ServiceUnavailable,
            [ErrorCode.InternalError] = StatusCodes.Status500InternalServerError
        };

    public static int Get(string? errorCode)
    {
        return Enum.TryParse<ErrorCode>(errorCode, out var code)
            && StatusCodesByError.TryGetValue(code, out var statusCode)
                ? statusCode
                : StatusCodesByError[ErrorCode.InternalError];
    }
}
