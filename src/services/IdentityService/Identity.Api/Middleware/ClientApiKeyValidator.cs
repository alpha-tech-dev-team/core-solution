using System.Security.Cryptography;
using System.Text;
using CoreSolution.Shared.Constants;
using Identity.Application.Errors;
using Identity.Api.Models;
using Microsoft.AspNetCore.Mvc;

namespace Identity.Api.Middleware;

internal static class ClientApiKeyValidator
{
    public static bool IsValid(HttpRequest request, ClientApiOptions options)
    {
        if (!request.Headers.TryGetValue(ClientApiRoutes.ServiceApiKeyHeader, out var suppliedValues)
            || suppliedValues.Count != 1
            || string.IsNullOrWhiteSpace(options.AtlasApiKey))
        {
            return false;
        }

        var suppliedKey = suppliedValues[0];

        if (string.IsNullOrEmpty(suppliedKey))
        {
            return false;
        }

        var expectedBytes = Encoding.UTF8.GetBytes(options.AtlasApiKey);
        var suppliedBytes = Encoding.UTF8.GetBytes(suppliedKey);

        return expectedBytes.Length == suppliedBytes.Length
            && CryptographicOperations.FixedTimeEquals(expectedBytes, suppliedBytes);
    }

    public static IActionResult Unauthorized()
    {
        return new ObjectResult(new ErrorResponse(nameof(IdentityErrorCode.InvalidCredentials)))
        {
            StatusCode = StatusCodes.Status401Unauthorized
        };
    }
}
