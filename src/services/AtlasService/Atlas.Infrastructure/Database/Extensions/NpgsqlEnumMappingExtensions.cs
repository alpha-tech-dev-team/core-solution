using Atlas.Infrastructure.Database.Constants;
using Atlas.Domain.Enums;
using Npgsql.EntityFrameworkCore.PostgreSQL.Infrastructure;

namespace Atlas.Infrastructure.Database.Extensions;

public static class NpgsqlEnumMappingExtensions
{
    public static NpgsqlDbContextOptionsBuilder MapAtlasEnums(this NpgsqlDbContextOptionsBuilder npgsql)
    {
        npgsql.MapEnum<EmployeeRole>(DatabaseConstants.EnumNames.EmployeeRole);
        npgsql.MapEnum<IncidentStatus>(DatabaseConstants.EnumNames.IncidentStatus);
        npgsql.MapEnum<IncidentPriority>(DatabaseConstants.EnumNames.IncidentPriority);
        npgsql.MapEnum<KnowledgeArticleStatus>(DatabaseConstants.EnumNames.KnowledgeArticleStatus);

        return npgsql;
    }
}

