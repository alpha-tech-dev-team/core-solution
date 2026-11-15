using Atlas.Application.Features.IncidentCategories;
using Atlas.Domain.Constants;
using Atlas.Domain.Enums;
using FluentValidation;

namespace Atlas.Application.Validators;

public sealed class SelectProblemCategoryMessageValidator : AbstractValidator<SelectProblemCategoryMessage>
{
    public SelectProblemCategoryMessageValidator()
    {
        RuleFor(message => message.TelegramUserId)
            .GreaterThan(ValidationConstants.TelegramUserIdMinExclusive)
            .WithErrorCode(nameof(ErrorCode.InvalidTelegramUserId));

        RuleFor(message => message.MessageId)
            .GreaterThan(ValidationConstants.MessageIdMinExclusive)
            .WithErrorCode(nameof(ErrorCode.InvalidMessageId));

        RuleFor(message => message.CategoryId)
            .NotEqual(Guid.Empty)
            .WithErrorCode(nameof(ErrorCode.InvalidCategoryId));
    }
}
