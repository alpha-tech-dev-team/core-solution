namespace Identity.Api.Interfaces;

public interface ICurrentUserService
{
    Guid UserId { get; }
    bool IsCurrentUser(Guid userId);
}
