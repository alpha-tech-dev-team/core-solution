using Atlas.Domain.Enums;

namespace Atlas.Application.Common;

public static class CallbackConstants
{
    public const string CategoryPrefix = nameof(WorkflowStep.ProblemCategorySelection) + ":category:";
    public const string CategoryPagePrefix = nameof(WorkflowStep.ProblemCategorySelection) + ":page:";
}
