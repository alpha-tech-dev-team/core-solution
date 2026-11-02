namespace Identity.Application.Errors;

public enum IdentityErrorCode
{
    UserNotFound,
    UserAlreadyActivated,
    AccessDenied,
    InvalidCredentials,
    InvalidRefreshToken,
    InternalError
}
