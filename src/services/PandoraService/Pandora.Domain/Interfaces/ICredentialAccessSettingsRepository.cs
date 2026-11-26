using Pandora.Domain.Models;

namespace Pandora.Domain.Interfaces;

public interface ICredentialAccessSettingsRepository
{
    Task<CredentialAccessSettings> Get(CancellationToken cancellationToken);
    Task<CredentialAccessSettings> Update(int ttlMinutes, Guid? updatedBy, CancellationToken cancellationToken);
}
