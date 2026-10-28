namespace Atlas.Core.Interfaces;

public interface IAuthorizedBotRequest
{
    long TelegramUserId { get; }
    long MessageId { get; }
}
