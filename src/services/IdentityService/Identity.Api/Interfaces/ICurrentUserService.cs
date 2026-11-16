namespace Identity.Api.Interfaces;

public interface ICurrentUserService
{
    Guid UserId { get; }
}
