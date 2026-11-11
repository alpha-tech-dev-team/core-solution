namespace Identity.Application.Errors;

public enum IdentityErrorCode
{
    UserNotFound,
    RoleNotFound,
    UserAlreadyActivated,
    PhoneAlreadyExists,
    AdminLoginAlreadyExists,
    AdminCredentialsRequired,
    AccessDenied,
    InvalidCredentials,
    InvalidRefreshToken,
    ValidationFailed,
    InternalError
}
