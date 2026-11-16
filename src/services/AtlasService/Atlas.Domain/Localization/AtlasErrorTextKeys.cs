using System.Collections.Frozen;
using Atlas.Domain.Constants.Localization;
using Atlas.Domain.Enums;
using CoreSolution.Shared.Constants;

namespace Atlas.Domain.Localization;

public static class AtlasErrorTextKeys
{
    public static readonly (string State, string Key) Fallback = (LocalizationConstants.IdleStateName, LocalizationConstants.Idle.UnexpectedError);

    public static readonly FrozenDictionary<string, (string State, string Key)> ByErrorCode = new Dictionary<string, (string State, string Key)>
    {
        [ErrorCodes.AccessNotConfirmed] = (LocalizationConstants.IdleStateName, LocalizationConstants.Idle.AccessNotConfirmed),
        [ErrorCodes.UserBlocked] = (LocalizationConstants.IdleStateName, LocalizationKeysConstants.Idle.UserBlocked),
        [nameof(ErrorCode.IncidentCategoriesNotFound)] = (nameof(WorkflowStep.ProblemCategorySelection), LocalizationKeysConstants.ProblemCategorySelection.CategoriesNotFound),
        [nameof(ErrorCode.IncidentCategoryNotFound)] = (nameof(WorkflowStep.ProblemCategorySelection), LocalizationKeysConstants.ProblemCategorySelection.CategoryNotFound),
        [nameof(ErrorCode.ValidationFailed)] = (LocalizationConstants.IdleStateName, LocalizationKeysConstants.Idle.ValidationFailed),
        [nameof(ErrorCode.InvalidTelegramUserId)] = (LocalizationConstants.IdleStateName, LocalizationKeysConstants.Idle.InvalidTelegramUserId),
        [nameof(ErrorCode.InvalidMessageId)] = (LocalizationConstants.IdleStateName, LocalizationKeysConstants.Idle.InvalidMessageId),
        [nameof(ErrorCode.InvalidPageNumber)] = (LocalizationConstants.IdleStateName, LocalizationKeysConstants.Idle.InvalidPageNumber),
        [nameof(ErrorCode.InvalidCategoryId)] = (LocalizationConstants.IdleStateName, LocalizationKeysConstants.Idle.InvalidCategoryId),
        [nameof(ErrorCode.RoomsNotFound)] = (nameof(WorkflowStep.RoomSelection), LocalizationKeysConstants.RoomSelection.RoomsNotFound),
        [nameof(ErrorCode.RoomNotFound)] = (nameof(WorkflowStep.RoomSelection), LocalizationKeysConstants.RoomSelection.RoomNotFound),
        [nameof(ErrorCode.InvalidRoomId)] = (LocalizationConstants.IdleStateName, LocalizationKeysConstants.Idle.InvalidRoomId),
        [nameof(ErrorCode.InvalidRoomText)] = (LocalizationConstants.IdleStateName, LocalizationKeysConstants.Idle.InvalidRoomText)
    }.ToFrozenDictionary();
}
