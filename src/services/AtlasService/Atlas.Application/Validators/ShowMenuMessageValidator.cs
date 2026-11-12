using Atlas.Application.Features.Menu;
using Atlas.Domain.Constants;
using FluentValidation;

namespace Atlas.Application.Validators;

public sealed class ShowMenuMessageValidator : AbstractValidator<ShowMenuMessage>
{
    public ShowMenuMessageValidator()
    {
        RuleFor(message => message.TelegramUserId)
            .GreaterThan(ValidationConstants.TelegramUserIdMinExclusive);

        RuleFor(message => message.MessageId)
            .GreaterThan(ValidationConstants.MessageIdMinExclusive);

        RuleFor(message => message.Page)
            .GreaterThanOrEqualTo(ValidationConstants.PageMinInclusive);
    }
}
