namespace Identity.Application.Contracts;

public record CheckUserRequest(long TelegramUserId, string Access);
