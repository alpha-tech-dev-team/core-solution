using Atlas.Domain.Constants;
using Atlas.Domain.Entities;
using Atlas.Domain.Models;
using Mapster;

namespace Atlas.Infrastructure.Mapping;

public sealed class IncidentMappingConfig : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<Incident, IncidentPageItem>()
            .Map(
                destination => destination.EmployeeFullName,
                source => source.Employee.Patronymic == null || source.Employee.Patronymic == string.Empty
                    ? source.Employee.Surname + EmployeeConstants.NameSeparator + source.Employee.Name
                    : source.Employee.Surname + EmployeeConstants.NameSeparator + source.Employee.Name + EmployeeConstants.NameSeparator + source.Employee.Patronymic)
            .Map(
                destination => destination.AssignedEngineerFullName,
                source => source.AssignedEngineer == null
                    ? null
                    : source.AssignedEngineer.Patronymic == null || source.AssignedEngineer.Patronymic == string.Empty
                        ? source.AssignedEngineer.Surname + EmployeeConstants.NameSeparator + source.AssignedEngineer.Name
                        : source.AssignedEngineer.Surname + EmployeeConstants.NameSeparator + source.AssignedEngineer.Name + EmployeeConstants.NameSeparator + source.AssignedEngineer.Patronymic);
    }
}
