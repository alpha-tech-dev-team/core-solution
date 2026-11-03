namespace CoreSolution.Shared.Abstractions;

public interface IAuthorizedBotRequest
{
    long TelegramUserId { get; }
    int MessageId { get; }
}
