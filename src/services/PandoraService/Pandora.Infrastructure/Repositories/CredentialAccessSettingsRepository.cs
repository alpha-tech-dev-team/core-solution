using Microsoft.EntityFrameworkCore;
using Pandora.Domain.Constants;
using Pandora.Domain.Entities;
using Pandora.Domain.Interfaces;
using Pandora.Domain.Models;
using Pandora.Infrastructure.Database;

namespace Pandora.Infrastructure.Repositories;

public sealed class CredentialAccessSettingsRepository : ICredentialAccessSettingsRepository
{
    private readonly PandoraDbContext _context;

    public CredentialAccessSettingsRepository(PandoraDbContext context)
    {
        _context = context;
    }

    public async Task<CredentialAccessSettings> Get(CancellationToken cancellationToken)
    {
        var setting = await _context.CredentialAccessSetting
            .AsNoTracking()
            .FirstOrDefaultAsync(item => item.Id == CredentialAccessConstants.SettingsRowId, cancellationToken);

        return setting is null
            ? new CredentialAccessSettings(CredentialAccessConstants.DefaultTtlMinutes, null)
            : new CredentialAccessSettings(setting.TtlMinutes, setting.UpdatedAt);
    }

    public async Task<CredentialAccessSettings> Update(
        int ttlMinutes,
        Guid? updatedBy,
        CancellationToken cancellationToken)
    {
        var setting = await _context.CredentialAccessSetting
            .FirstOrDefaultAsync(item => item.Id == CredentialAccessConstants.SettingsRowId, cancellationToken);
        var updatedAt = DateTime.UtcNow;

        if (setting is null)
        {
            setting = new CredentialAccessSetting { Id = CredentialAccessConstants.SettingsRowId };

            _context.CredentialAccessSetting.Add(setting);
        }

        setting.TtlMinutes = ttlMinutes;
        setting.UpdatedAt = updatedAt;
        setting.UpdatedBy = updatedBy;

        await _context.SaveChangesAsync(cancellationToken);

        return new CredentialAccessSettings(setting.TtlMinutes, updatedAt);
    }
}
