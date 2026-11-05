namespace Identity.Application.Contracts.Client;

public record ActivateUserRequest(long TelegramUserId, string Phone);
