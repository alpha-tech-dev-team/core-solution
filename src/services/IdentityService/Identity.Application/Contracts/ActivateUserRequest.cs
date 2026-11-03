namespace Identity.Application.Contracts;

public record ActivateUserRequest(long TelegramUserId, string Phone);
