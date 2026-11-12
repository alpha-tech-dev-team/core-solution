using Identity.Domain.Enums;

namespace Identity.Application.Contracts.Admin;

public record SetUserStatusRequest(UserStatus Status);
