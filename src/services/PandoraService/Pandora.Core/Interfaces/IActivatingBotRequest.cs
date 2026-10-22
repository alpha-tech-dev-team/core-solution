namespace Pandora.Core.Interfaces;

public interface IActivatingBotRequest
{
    long TelegramUserId { get; }

    int MessageId { get; }

    string Phone { get; }
}
