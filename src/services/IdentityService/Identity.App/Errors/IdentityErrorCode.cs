namespace Identity.App.Errors;

public enum IdentityErrorCode
{
    UserNotFound,
    UserAlreadyActivated,
    AccessDenied,
    InvalidCredentials,
    InvalidRefreshToken,
    InternalError
}
