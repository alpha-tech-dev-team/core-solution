using Atlas.Domain.Constants;
using Atlas.Domain.Entities;
using Atlas.Domain.Interfaces.Repositories;
using Atlas.Domain.Models;
using Atlas.Infrastructure.Database.Context;
using Microsoft.EntityFrameworkCore;

namespace Atlas.Infrastructure.Repositories;

public sealed class IncidentRepository : BaseRepository<Incident>, IIncidentRepository
{
    public IncidentRepository(AtlasDbContext context)
        : base(context)
    {
    }

    public async Task AddWithStatusHistoryAsync(Incident incident, CancellationToken cancellationToken = default)
    {
        Entities.Add(incident);

        Context.IncidentStatusHistory.Add(new IncidentStatusHistory
        {
            Incident = incident,
            Status = incident.Status,
            CreatedAt = incident.CreatedAt
        });

        await Context.SaveChangesAsync(cancellationToken);
    }

    public async Task<IncidentListPage> GetListPageAsync(
        IncidentPageQuery query,
        CancellationToken cancellationToken = default)
    {
        var source = Entities.AsNoTracking();
        var total = await source.CountAsync(cancellationToken);

        var rows = await query.OrderBy(source)
            .Skip(query.Paging.Skip)
            .Take(query.Paging.EffectivePageSize)
            .Select(incident => new
            {
                incident.Id,
                incident.TicketNumber,
                EmployeeSurname = incident.Employee.Surname,
                EmployeeName = incident.Employee.Name,
                EmployeePatronymic = incident.Employee.Patronymic,
                CategoryName = incident.Category.Name,
                RoomName = incident.Room != null ? incident.Room.Name : null,
                HardwareName = incident.Hardware != null ? incident.Hardware.Name : null,
                SoftwareName = incident.Software != null ? incident.Software.Name : null,
                incident.UserDescription,
                incident.Status,
                incident.Priority,
                EngineerSurname = incident.AssignedEngineer != null ? incident.AssignedEngineer.Surname : null,
                EngineerName = incident.AssignedEngineer != null ? incident.AssignedEngineer.Name : null,
                EngineerPatronymic = incident.AssignedEngineer != null ? incident.AssignedEngineer.Patronymic : null,
                incident.CreatedAt
            })
            .ToListAsync(cancellationToken);

        var items = rows
            .Select(row => new IncidentPageItem(
                row.Id,
                row.TicketNumber,
                FormatFullName(row.EmployeeSurname, row.EmployeeName, row.EmployeePatronymic),
                row.CategoryName,
                row.RoomName,
                row.HardwareName,
                row.SoftwareName,
                row.UserDescription,
                row.Status,
                row.Priority,
                row.EngineerSurname is null || row.EngineerName is null
                    ? null
                    : FormatFullName(row.EngineerSurname, row.EngineerName, row.EngineerPatronymic),
                row.CreatedAt))
            .ToList();

        return new IncidentListPage(items, query.Paging.EffectivePage, query.Paging.EffectivePageSize, total);
    }

    private static string FormatFullName(string surname, string name, string? patronymic)
    {
        return string.IsNullOrWhiteSpace(patronymic)
            ? string.Concat(surname, EmployeeConstants.NameSeparator, name)
            : string.Concat(surname, EmployeeConstants.NameSeparator, name, EmployeeConstants.NameSeparator, patronymic);
    }
}
