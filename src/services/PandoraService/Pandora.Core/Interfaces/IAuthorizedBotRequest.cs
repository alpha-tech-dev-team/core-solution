namespace Pandora.Core.Interfaces;

public interface IAuthorizedBotRequest
{
    long TelegramUserId { get; }
    int MessageId { get; }
}
