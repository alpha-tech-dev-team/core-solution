using Atlas.Application.Features.Menu;
using Atlas.Domain.Constants;
using Atlas.Domain.Enums;
using FluentValidation;

namespace Atlas.Application.Validators;

public sealed class ShowStartMenuMessageValidator : AbstractValidator<ShowStartMenuMessage>
{
    public ShowStartMenuMessageValidator()
    {
        RuleFor(message => message.TelegramUserId)
            .GreaterThan(ValidationConstants.TelegramUserIdMinExclusive)
            .WithErrorCode(nameof(ErrorCode.InvalidTelegramUserId));

        RuleFor(message => message.MessageId)
            .GreaterThan(ValidationConstants.MessageIdMinExclusive)
            .WithErrorCode(nameof(ErrorCode.InvalidMessageId));
    }
}
