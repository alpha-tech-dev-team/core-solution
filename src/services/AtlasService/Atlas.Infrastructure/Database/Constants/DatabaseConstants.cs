namespace Atlas.Infrastructure.Database.Constants;

public static class DatabaseConstants
{
    public static class EnumNames
    {
        public const string EmployeeRole = "employee_role";
        public const string IncidentStatus = "incident_status";
        public const string IncidentPriority = "incident_priority";
        public const string KnowledgeArticleStatus = "knowledge_article_status";
    }

    public static class Room
    {
        public const string Table = "rooms";
        public const string Id = "id";
        public const string Name = "name";
        public const string Description = "description";
        public const string CreatedAt = "created_at";
        public const int NameMaxLength = 50;
    }

    public static class Hardware
    {
        public const string Table = "hardware";
        public const string Id = "id";
        public const string Name = "name";
        public const string Type = "type";
        public const string InventoryNumber = "inventory_number";
        public const string SerialNumber = "serial_number";
        public const string RoomId = "room_id";
        public const string CreatedAt = "created_at";
        public const string UpdatedAt = "updated_at";
        public const int NameMaxLength = 255;
        public const int TypeMaxLength = 100;
        public const int InventoryNumberMaxLength = 100;
        public const int SerialNumberMaxLength = 255;
    }

    public static class Software
    {
        public const string Table = "software";
        public const string Id = "id";
        public const string Name = "name";
        public const string Version = "version";
        public const int NameMaxLength = 255;
        public const int VersionMaxLength = 100;
    }

    public static class Employee
    {
        public const string Table = "employees";
        public const string Id = "id";
        public const string Role = "role";
        public const string CreatedAt = "created_at";
        public const string UpdatedAt = "updated_at";
    }

    public static class IncidentCategory
    {
        public const string Table = "incident_categories";
        public const string Id = "id";
        public const string Name = "name";
        public const string Description = "description";
        public const int NameMaxLength = 100;
    }

    public static class Incident
    {
        public const string Table = "incidents";
        public const string Id = "id";
        public const string TicketNumber = "ticket_number";
        public const string EmployeeId = "employee_id";
        public const string CategoryId = "category_id";
        public const string RoomId = "room_id";
        public const string HardwareId = "hardware_id";
        public const string SoftwareId = "software_id";
        public const string UserDescription = "user_description";
        public const string Status = "status";
        public const string Priority = "priority";
        public const string AssignedEngineerId = "assigned_engineer_id";
        public const string ArticleId = "article_id";
        public const string Resolution = "resolution";
        public const string ResolvedAt = "resolved_at";
        public const string CreatedAt = "created_at";
        public const string UpdatedAt = "updated_at";
    }

    public static class IncidentFeedback
    {
        public const string Table = "incident_feedback";
        public const string Id = "id";
        public const string IncidentId = "incident_id";
        public const string ArticleId = "article_id";
        public const string WasHelpful = "was_helpful";
        public const string CreatedAt = "created_at";
    }

    public static class IncidentStatusHistory
    {
        public const string Table = "incident_status_history";
        public const string Id = "id";
        public const string IncidentId = "incident_id";
        public const string Status = "status";
        public const string LastChangedBy = "last_changed_by";
        public const string Comment = "comment";
        public const string CreatedAt = "created_at";
    }

    public static class KnowledgeArticle
    {
        public const string Table = "knowledge_articles";
        public const string Id = "id";
        public const string CategoryId = "category_id";
        public const string Title = "title";
        public const string Symptoms = "symptoms";
        public const string Solution = "solution";
        public const string Status = "status";
        public const string CreatedBy = "created_by";
        public const string SourceIncidentId = "source_incident_id";
        public const string CreatedAt = "created_at";
        public const string UpdatedAt = "updated_at";
        public const int TitleMaxLength = 255;
    }

    public static class KnowledgeArticleVector
    {
        public const string Table = "knowledge_article_vectors";
        public const string ArticleId = "article_id";
        public const string Embedding = "embedding";
        public const string EmbeddingModel = "embedding_model";
        public const string EmbeddedText = "embedded_text";
        public const int EmbeddingModelMaxLength = 100;
        public const string EmbeddingColumnType = "vector(1536)";
        public const string EmbeddingIndexMethod = "hnsw";
        public const string EmbeddingIndexOperator = "vector_cosine_ops";
    }

    public static class Defaults
    {
        public const string NowSql = "now()";
        public const string GenRandomUuidSql = "gen_random_uuid()";
        public const string VectorExtension = "vector";
    }
}


