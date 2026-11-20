using Atlas.Domain.Entities;

namespace Atlas.Domain.Interfaces.Repositories;

public interface IEmployeeRepository
{
    Task<Employee> GetOrCreateByTelegramUserIdAsync(
        long telegramUserId,
        string? fallbackName,
        string? fallbackSurname,
        CancellationToken cancellationToken = default);
}
