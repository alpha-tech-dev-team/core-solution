using Atlas.Domain.Models;

namespace Atlas.Domain.Interfaces;

public interface IEmployeeDirectory
{
    Task<EmployeeProfile?> FindByTelegramUserIdAsync(long telegramUserId, CancellationToken cancellationToken = default);
}
