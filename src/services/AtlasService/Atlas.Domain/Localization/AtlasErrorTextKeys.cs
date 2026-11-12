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
        [nameof(ErrorCode.IncidentCategoriesNotFound)] = (nameof(WorkflowStep.ProblemCategorySelection), LocalizationKeysConstants.ProblemCategorySelection.CategoriesNotFound),
        [nameof(ErrorCode.IncidentCategoryNotFound)] = (nameof(WorkflowStep.ProblemCategorySelection), LocalizationKeysConstants.ProblemCategorySelection.CategoryNotFound)
    }.ToFrozenDictionary();
}
