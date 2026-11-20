using Atlas.Domain.Entities;
using Atlas.Domain.Enums;
using Atlas.Domain.Interfaces.Repositories;
using Atlas.Domain.Models;
using CoreSolution.Shared.Results;

namespace Atlas.Application.Features.Incidents;

public sealed class IncidentCreationService
{
    private readonly IIncidentRepository _incidentRepository;
    private readonly IEmployeeRepository _employeeRepository;
    private readonly IIncidentCategoryRepository _categoryRepository;
    private readonly IHardwareRepository _hardwareRepository;
    private readonly ISoftwareRepository _softwareRepository;

    public IncidentCreationService(
        IIncidentRepository incidentRepository,
        IEmployeeRepository employeeRepository,
        IIncidentCategoryRepository categoryRepository,
        IHardwareRepository hardwareRepository,
        ISoftwareRepository softwareRepository)
    {
        _incidentRepository = incidentRepository;
        _employeeRepository = employeeRepository;
        _categoryRepository = categoryRepository;
        _hardwareRepository = hardwareRepository;
        _softwareRepository = softwareRepository;
    }

    public async Task<Result<Incident>> CreateAsync(
        AtlasUserSession session,
        long telegramUserId,
        string? employeeName,
        string? employeeSurname,
        CancellationToken cancellationToken)
    {
        if (session.IncidentCategoryId is not Guid categoryId
            || session.RoomId is not Guid roomId
            || string.IsNullOrWhiteSpace(session.IncidentDescription))
        {
            return Result<Incident>.Failure(nameof(ErrorCode.IncidentDescriptionNotSet));
        }

        var category = await _categoryRepository.GetByIdAsync(categoryId, cancellationToken);

        if (category is null || !IsAssetSelectionValid(category.Type, session))
        {
            return Result<Incident>.Failure(nameof(ErrorCode.IncidentContextIncomplete));
        }

        if (session.HardwareId is Guid hardwareId
            && await _hardwareRepository.GetByIdInRoomAsync(hardwareId, roomId, cancellationToken) is null)
        {
            return Result<Incident>.Failure(nameof(ErrorCode.HardwareNotFound));
        }

        if (session.SoftwareId is Guid softwareId
            && await _softwareRepository.GetByIdAsync(softwareId, cancellationToken) is null)
        {
            return Result<Incident>.Failure(nameof(ErrorCode.SoftwareNotFound));
        }

        var employee = await _employeeRepository.GetOrCreateByTelegramUserIdAsync(
            telegramUserId,
            employeeName,
            employeeSurname,
            cancellationToken);

        var now = DateTime.UtcNow;

        var incident = new Incident
        {
            Id = Guid.NewGuid(),
            EmployeeId = employee.Id,
            CategoryId = categoryId,
            RoomId = roomId,
            HardwareId = session.HardwareId,
            SoftwareId = session.SoftwareId,
            UserDescription = session.IncidentDescription,
            Status = IncidentStatus.Created,
            Priority = IncidentPriority.Medium,
            CreatedAt = now,
            UpdatedAt = now
        };

        await _incidentRepository.AddWithStatusHistoryAsync(incident, cancellationToken);

        return Result<Incident>.Success(incident);
    }

    private static bool IsAssetSelectionValid(IncidentCategoryType type, AtlasUserSession session)
    {
        return type switch
        {
            IncidentCategoryType.Hardware => session.HardwareId is not null && session.SoftwareId is null,
            IncidentCategoryType.Software => session.SoftwareId is not null && session.HardwareId is null,
            _ => session.HardwareId is null && session.SoftwareId is null
        };
    }
}
