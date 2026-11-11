namespace Identity.Application.Contracts.Client;

public record CheckUserRequest(long TelegramUserId, string Access);
