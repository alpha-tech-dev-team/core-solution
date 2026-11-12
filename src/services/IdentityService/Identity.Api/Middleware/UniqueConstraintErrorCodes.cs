using Identity.Application.Errors;
using Identity.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace Identity.Api.Middleware;

public static class UniqueConstraintErrorCodes
{
    private static readonly IReadOnlyDictionary<string, string> ErrorCodesByConstraint =
        new Dictionary<string, string>
        {
            [DatabaseConstants.Users.PhoneIndex] = nameof(IdentityErrorCode.PhoneAlreadyExists),
            [DatabaseConstants.Users.AdminLoginIndex] = nameof(IdentityErrorCode.AdminLoginAlreadyExists),
            [DatabaseConstants.Users.TelegramUserIdIndex] = nameof(IdentityErrorCode.UserAlreadyActivated)
        };

    public static bool TryGetErrorCode(Exception exception, out string errorCode)
    {
        errorCode = string.Empty;

        if (exception is not DbUpdateException { InnerException: PostgresException { SqlState: PostgresErrorCodes.UniqueViolation } postgres })
        {
            return false;
        }

        if (postgres.ConstraintName is null || !ErrorCodesByConstraint.TryGetValue(postgres.ConstraintName, out var mappedCode))
        {
            return false;
        }

        errorCode = mappedCode;
        return true;
    }
}
