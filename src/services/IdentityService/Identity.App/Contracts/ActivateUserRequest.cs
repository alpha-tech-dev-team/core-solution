namespace Identity.App.Contracts;

public record ActivateUserRequest(long TelegramUserId, string Phone);
