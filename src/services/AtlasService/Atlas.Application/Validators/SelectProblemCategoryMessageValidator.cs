using Atlas.Application.Features.IncidentCategories;
using Atlas.Domain.Constants;
using FluentValidation;

namespace Atlas.Application.Validators;

public sealed class SelectProblemCategoryMessageValidator : AbstractValidator<SelectProblemCategoryMessage>
{
    public SelectProblemCategoryMessageValidator()
    {
        RuleFor(message => message.TelegramUserId)
            .GreaterThan(ValidationConstants.TelegramUserIdMinExclusive);

        RuleFor(message => message.MessageId)
            .GreaterThan(ValidationConstants.MessageIdMinExclusive);

        RuleFor(message => message.CategoryId)
            .NotEqual(Guid.Empty);
    }
}
