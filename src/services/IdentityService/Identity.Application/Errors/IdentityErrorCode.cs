namespace Identity.Application.Errors;

public enum IdentityErrorCode
{
    UserNotFound,
    RoleNotFound,
    UserAlreadyActivated,
    PhoneAlreadyExists,
    AccessDenied,
    InvalidCredentials,
    InvalidRefreshToken,
    ValidationFailed,
    InternalError
}
