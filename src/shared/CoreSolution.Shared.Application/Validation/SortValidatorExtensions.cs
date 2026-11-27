using CoreSolution.Shared.Models;
using FluentValidation;

namespace CoreSolution.Shared.Application.Validation;

public static class SortValidatorExtensions
{
    public static void ApplySortRules<TMessage>(
        this AbstractValidator<TMessage> validator,
        IReadOnlyCollection<string> supportedFields,
        string? fieldErrorCode = null,
        string? directionErrorCode = null)
        where TMessage : ISortMessage
    {
        var fieldRule = validator.RuleFor(message => message.SortBy)
            .Must(field => field is null || supportedFields.Contains(field, StringComparer.OrdinalIgnoreCase));

        if (fieldErrorCode is not null)
        {
            fieldRule.WithErrorCode(fieldErrorCode);
        }

        var directionRule = validator.RuleFor(message => message.SortDirection)
            .Must(direction => direction is null || Enum.IsDefined(direction.Value));

        if (directionErrorCode is not null)
        {
            directionRule.WithErrorCode(directionErrorCode);
        }
    }
}
