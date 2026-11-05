namespace Identity.Application.Errors;

public enum IdentityErrorCode
{
    UserNotFound,
    RoleNotFound,
    UserAlreadyActivated,
    AccessDenied,
    InvalidCredentials,
    InvalidRefreshToken,
    InternalError
}
