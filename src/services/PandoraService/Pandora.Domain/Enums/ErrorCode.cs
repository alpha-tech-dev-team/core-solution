namespace Pandora.Domain.Enums;

public enum ErrorCode
{
    AccessDenied,
    AccessNotConfirmed,
    SecretNotFound,
    InvalidSecretData,
    VaultwardenUnavailable,
    DatabaseUnavailable,
    Conflict,
    MalformedRequest,
    InvalidPage,
    InvalidPageSize,
    InvalidSortField,
    InvalidSortDirection,
    InvalidSearch,
    InternalError
}
