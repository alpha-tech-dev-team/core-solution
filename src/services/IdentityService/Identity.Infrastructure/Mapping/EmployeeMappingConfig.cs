using Identity.Domain.Entities;
using Identity.Domain.Models;
using Mapster;

namespace Identity.Infrastructure.Mapping;

public sealed class EmployeeMappingConfig : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<User, EmployeeInfo>()
            .Map(destination => destination.Status, source => source.Status.ToString())
            .Map(destination => destination.Roles, source => source.UserRoles.Select(role => role.Role.Name).ToArray());
    }
}
