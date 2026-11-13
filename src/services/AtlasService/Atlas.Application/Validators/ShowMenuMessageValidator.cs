using Atlas.Application.Features.Menu;
using Atlas.Domain.Constants;
using Atlas.Domain.Enums;
using FluentValidation;

namespace Atlas.Application.Validators;

public sealed class ShowMenuMessageValidator : AbstractValidator<ShowMenuMessage>
{
    public ShowMenuMessageValidator()
    {
        RuleFor(message => message.TelegramUserId)
            .GreaterThan(ValidationConstants.TelegramUserIdMinExclusive)
            .WithErrorCode(nameof(ErrorCode.InvalidTelegramUserId));

        RuleFor(message => message.MessageId)
            .GreaterThan(ValidationConstants.MessageIdMinExclusive)
            .WithErrorCode(nameof(ErrorCode.InvalidMessageId));

        RuleFor(message => message.Page)
            .GreaterThanOrEqualTo(ValidationConstants.PageMinInclusive)
            .WithErrorCode(nameof(ErrorCode.InvalidPageNumber));
    }
}
