namespace Atlas.Domain.Enums;

public enum ErrorCode
{
    AccessDenied,
    AccessNotConfirmed,
    SessionNotFound,
    IncidentCategoriesNotFound,
    IncidentCategoryNotFound,
    ValidationFailed,
    InvalidTelegramUserId,
    InvalidMessageId,
    InvalidPageNumber,
    InvalidCategoryId,
    InvalidHardwareId,
    InvalidSoftwareId,
    RoomsNotFound,
    RoomNotFound,
    InvalidRoomId,
    InvalidRoomText,
    HardwareNotFound,
    HardwareNotConfigured,
    SoftwareNotFound,
    SoftwareNotConfigured,
    InvalidIncidentDescription,
    IncidentDescriptionNotSet,
    IncidentContextIncomplete,
    IncidentStepNotSupported,
    DatabaseUnavailable,
    Conflict,
    MalformedRequest,
    InternalError
}
