using Atlas.Domain.Constants;
using Atlas.Domain.Entities;
using Atlas.Domain.Enums;
using Atlas.Domain.Interfaces;
using Atlas.Domain.Interfaces.Repositories;
using Atlas.Domain.Models;
using Atlas.Infrastructure.Database.Context;
using Microsoft.EntityFrameworkCore;

namespace Atlas.Infrastructure.Repositories;

public sealed class EmployeeRepository : IEmployeeRepository
{
    private readonly AtlasDbContext _context;
    private readonly IEmployeeDirectory _employeeDirectory;

    public EmployeeRepository(AtlasDbContext context, IEmployeeDirectory employeeDirectory)
    {
        _context = context;
        _employeeDirectory = employeeDirectory;
    }

    public async Task<Employee> GetOrCreateByTelegramUserIdAsync(
        long telegramUserId,
        string? fallbackName,
        string? fallbackSurname,
        CancellationToken cancellationToken = default)
    {
        var directoryProfile = await _employeeDirectory.FindByTelegramUserIdAsync(telegramUserId, cancellationToken);

        var profile = directoryProfile ?? new EmployeeProfile(
            string.IsNullOrWhiteSpace(fallbackName) ? EmployeeConstants.FallbackName : fallbackName,
            string.IsNullOrWhiteSpace(fallbackSurname) ? EmployeeConstants.FallbackSurname : fallbackSurname,
            null);

        var employee = await _context.Employees
            .FirstOrDefaultAsync(candidate => candidate.TelegramUserId == telegramUserId, cancellationToken);

        if (employee is null)
        {
            employee = new Employee
            {
                Role = EmployeeRole.Employee,
                TelegramUserId = telegramUserId,
                Name = profile.Name,
                Surname = profile.Surname,
                Patronymic = profile.Patronymic,
                CreatedAt = DateTime.UtcNow
            };

            _context.Employees.Add(employee);
        }
        else
        {
            employee.Name = profile.Name;
            employee.Surname = profile.Surname;
            employee.Patronymic = profile.Patronymic;
        }

        await _context.SaveChangesAsync(cancellationToken);

        return employee;
    }
}
