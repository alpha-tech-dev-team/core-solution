namespace Identity.Application.Errors;

public enum IdentityErrorCode
{
    UserNotFound,
    RoleNotFound,
    UserAlreadyActivated,
    UserBlocked,
    PhoneAlreadyExists,
    AdminLoginAlreadyExists,
    AdminCredentialsRequired,
    AccessDenied,
    InvalidCredentials,
    InvalidRefreshToken,
    ValidationFailed,
    SelfActionForbidden,
    InternalError
}
