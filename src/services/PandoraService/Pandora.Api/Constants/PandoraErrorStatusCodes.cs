using Microsoft.AspNetCore.Http;
using Pandora.Domain.Enums;

namespace Pandora.Api.Constants;

internal static class PandoraErrorStatusCodes
{
    private static readonly IReadOnlyDictionary<ErrorCode, int> StatusCodesByError =
        new Dictionary<ErrorCode, int>
        {
            [ErrorCode.InvalidSecretData] = StatusCodes.Status400BadRequest,
            [ErrorCode.SecretNotFound] = StatusCodes.Status404NotFound,
            [ErrorCode.VaultwardenUnavailable] = StatusCodes.Status503ServiceUnavailable,
            [ErrorCode.DatabaseUnavailable] = StatusCodes.Status503ServiceUnavailable
        };

    public static int Get(string? errorCode)
    {
        return Enum.TryParse<ErrorCode>(errorCode, out var code)
            && StatusCodesByError.TryGetValue(code, out var statusCode)
                ? statusCode
                : StatusCodes.Status500InternalServerError;
    }
}
