namespace Pandora.Application.Common.Interfaces;

public interface IUserActionMessage
{
    long TelegramUserId { get; }
    string Action { get; }
    string? TargetItemId { get; }
}
