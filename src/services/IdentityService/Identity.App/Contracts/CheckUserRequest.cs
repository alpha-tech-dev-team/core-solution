namespace Identity.App.Contracts;

public record CheckUserRequest(long TelegramUserId, string Access);
