namespace Identity.Application.Errors;

public enum IdentityErrorCode
{
    UserNotFound,
    RoleNotFound,
    UserAlreadyActivated,
    PhoneAlreadyExists,
    AdminCredentialsRequired,
    AccessDenied,
    InvalidCredentials,
    InvalidRefreshToken,
    ValidationFailed,
    InternalError
}
