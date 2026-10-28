using Microsoft.Extensions.Options;
using Atlas.Core.Entities;
using Atlas.Core.Interfaces;
using Atlas.Core.Options;

namespace Atlas.App.Services;

public sealed class SessionService : ISessionService
{
    private readonly ISessionRepository _repository;
    private readonly SessionOptions _options;

    public SessionService(ISessionRepository repository, IOptions<SessionOptions> options)
    {
        _repository = repository;
        _options = options.Value;
    }

    public async Task<UserSession?> GetSession(long telegramUserId, CancellationToken cancellationToken)
    {
        return await _repository.GetSession(telegramUserId, cancellationToken);
    }

    public async Task SaveSession(UserSession session, CancellationToken cancellationToken)
    {
        var expiry = TimeSpan.FromMinutes(_options.ExpiryMinutes);

        await _repository.SaveSession(session, expiry, cancellationToken);
        await _repository.AddToIndex(session.TgUserId, DateTimeOffset.UtcNow, cancellationToken);
    }

    public async Task DeleteSession(long telegramUserId, CancellationToken cancellationToken)
    {
        await _repository.DeleteSession(telegramUserId, cancellationToken);
        await _repository.RemoveFromIndex(telegramUserId, cancellationToken);
    }
}
