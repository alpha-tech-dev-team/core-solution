namespace Atlas.Domain.Constants.AtlasDBConfiguration;

public static class DBKeys
{
    public static class Shared
    {
        public const string Schema = "atlas";
        public const string Id = "id";
        public const string IsActive = "is_active";
        public const string CreatedAt = "created_at";
        public const string UpdatedAt = "updated_at";
        public const string NowSql = "now()";
        public const string GenRandomUuidSql = "gen_random_uuid()";
        public const string TimestampWithTimeZone = "timestamp with time zone";
        public const string VectorExtension = "vector";
        public const string MigrationsHistoryTable = "__EFMigrationsHistory";
        public const int MaxLength255 = 255;
        public const int MaxLength100 = 100;
        public const int MaxLength50 = 50;
        public const int MaxLength30 = 30;
        public const int MaxLength20 = 20;
    }

    public static class Employee
    {
        public const string Table = "employees";
        public const string EmployeeId = "employee_id";
        public const string TelegramId = "telegram_id";
        public const string Role = "role";
        public const string PrimaryKey = "employees_pkey";
        public const string RoleCheckConstraint = "chk_employee_role";
        public const string RoleCheckSql = "role IN ('employee', 'engineer', 'admin')";
        public const string RoleDefaultValue = "employee";
        public const string TelegramIdUniqueIndex = "employees_telegram_id_key";
    }

    public static class Hardware
    {
        public const string Table = "hardware";
        public const string Name = "name";
        public const string Type = "type";
        public const string InventoryNumber = "inventory_number";
        public const string SerialNumber = "serial_number";
        public const string CabinetId = "cabinet_id";
        public const string PrimaryKey = "hardware_pkey";
        public const string InventoryNumberUniqueIndex = "hardware_inventory_number_key";
        public const string CabinetTypeIndex = "idx_hardware_cabinet_type";
        public const string CabinetForeignKey = "hardware_cabinet_id_fkey";
    }

    public static class IncidentCategory
    {
        public const string Table = "incident_categories";
        public const string Name = "name";
        public const string Description = "description";
        public const string PrimaryKey = "incident_categories_pkey";
        public const string NameUniqueIndex = "incident_categories_name_key";
        public const string HardwareId = "0f8fad5b-d9cb-469f-a165-70867728950e";
        public const string SoftwareId = "7c9e6679-7425-40de-944b-e07fc1f90ae7";
        public const string OtherId = "f47ac10b-58cc-4372-a567-0e02b2c3d479";
        public const string HardwareName = "hardware";
        public const string SoftwareName = "software";
        public const string OtherName = "other";
    }

    public static class Incident
    {
        public const string Table = "incidents";
        public const string TicketNumber = "ticket_number";
        public const string EmployeeId = "employee_id";
        public const string CategoryId = "category_id";
        public const string CabinetId = "cabinet_id";
        public const string HardwareId = "hardware_id";
        public const string SoftwareId = "software_id";
        public const string UserDescription = "user_description";
        public const string Status = "status";
        public const string Priority = "priority";
        public const string AssignedEngineerId = "assigned_engineer_id";
        public const string ArticleId = "article_id";
        public const string Resolution = "resolution";
        public const string ResolvedAt = "resolved_at";
        public const string PrimaryKey = "incidents_pkey";
        public const string StatusCheckConstraint = "chk_incident_status";
        public const string StatusCheckSql = "status IN ('created', 'in_progress', 'resolved', 'resolved_automatically', 'closed', 'cancelled')";
        public const string StatusCreated = "created";
        public const string StatusInProgress = "in_progress";
        public const string StatusResolved = "resolved";
        public const string StatusResolvedAutomatically = "resolved_automatically";
        public const string StatusClosed = "closed";
        public const string StatusCancelled = "cancelled";
        public const string PriorityCheckConstraint = "chk_incident_priority";
        public const string PriorityCheckSql = "priority IN ('low', 'medium', 'high', 'critical')";
        public const string DescriptionCheckConstraint = "chk_incident_description";
        public const string DescriptionCheckSql = "length(trim(user_description)) > 0";
        public const string ResolutionCheckConstraint = "chk_incident_resolution";
        public const string ResolutionCheckSql = "status NOT IN ('resolved', 'resolved_automatically') OR (resolution IS NOT NULL AND length(trim(resolution)) > 0)";
        public const string AssigneeCheckConstraint = "chk_incident_assignee";
        public const string AssigneeCheckSql = "status <> 'in_progress' OR assigned_engineer_id IS NOT NULL";
        public const string AssetCheckConstraint = "chk_incident_asset";
        public const string AssetCheckSql = "hardware_id IS NULL OR software_id IS NULL";
        public const string StatusDefaultValue = StatusCreated;
        public const string PriorityDefaultValue = "medium";
        public const string AssignedEngineerIndex = "idx_incidents_assigned_engineer";
        public const string CategoryIndex = "idx_incidents_category";
        public const string CreatedAtIndex = "idx_incidents_created_at";
        public const string EmployeeIndex = "idx_incidents_employee";
        public const string StatusPriorityIndex = "idx_incidents_status_priority";
        public const string EmployeeForeignKey = "incidents_employee_id_fkey";
        public const string AssignedEngineerForeignKey = "incidents_assigned_engineer_id_fkey";
        public const string CategoryForeignKey = "incidents_category_id_fkey";
        public const string CabinetForeignKey = "incidents_cabinet_id_fkey";
        public const string HardwareForeignKey = "incidents_hardware_id_fkey";
        public const string SoftwareForeignKey = "incidents_software_id_fkey";
        public const string ArticleForeignKey = "incidents_article_id_fkey";
    }

    public static class IncidentFeedback
    {
        public const string Table = "incident_feedback";
        public const string IncidentId = "incident_id";
        public const string ArticleId = "article_id";
        public const string WasHelpful = "was_helpful";
        public const string PrimaryKey = "incident_feedback_pkey";
        public const string IncidentArticleUniqueIndex = "uq_incident_feedback";
        public const string ArticleIndex = "idx_incident_feedback_article";
        public const string IncidentForeignKey = "incident_feedback_incident_id_fkey";
        public const string ArticleForeignKey = "incident_feedback_article_id_fkey";
    }

    public static class IncidentStatusHistory
    {
        public const string Table = "incident_status_history";
        public const string IncidentId = "incident_id";
        public const string OldStatus = "old_status";
        public const string NewStatus = "new_status";
        public const string ChangedBy = "changed_by";
        public const string Comment = "comment";
        public const string PrimaryKey = "incident_status_history_pkey";
        public const string IncidentCreatedAtIndex = "idx_incident_status_history_incident";
        public const string IncidentForeignKey = "incident_status_history_incident_id_fkey";
        public const string ChangedByForeignKey = "incident_status_history_changed_by_fkey";
    }

    public static class KnowledgeArticle
    {
        public const string Table = "knowledge_articles";
        public const string CategoryId = "category_id";
        public const string Title = "title";
        public const string Symptoms = "symptoms";
        public const string Solution = "solution";
        public const string Status = "status";
        public const string CreatedBy = "created_by";
        public const string SourceIncidentId = "source_incident_id";
        public const string PrimaryKey = "knowledge_articles_pkey";
        public const string StatusCheckConstraint = "chk_article_status";
        public const string StatusCheckSql = "status IN ('draft', 'published', 'archived')";
        public const string TitleCheckConstraint = "chk_article_title";
        public const string TitleCheckSql = "length(trim(title)) > 0";
        public const string SolutionCheckConstraint = "chk_article_solution";
        public const string SolutionCheckSql = "length(trim(solution)) > 0";
        public const string StatusDefaultValue = "draft";
        public const string CategoryIndex = "idx_knowledge_articles_category";
        public const string StatusIndex = "idx_knowledge_articles_status";
        public const string SourceIncidentUniqueIndex = "knowledge_articles_source_incident_id_key";
        public const string CategoryForeignKey = "knowledge_articles_category_id_fkey";
        public const string CreatedByForeignKey = "knowledge_articles_created_by_fkey";
        public const string SourceIncidentForeignKey = "knowledge_articles_source_incident_id_fkey";
    }

    public static class KnowledgeArticleVector
    {
        public const string Table = "knowledge_article_vectors";
        public const string ArticleId = "article_id";
        public const string Embedding = "embedding";
        public const string EmbeddingModel = "embedding_model";
        public const string EmbeddedText = "embedded_text";
        public const string PrimaryKey = "knowledge_article_vectors_pkey";
        public const string EmbeddingColumnType = "vector(1536)";
        public const string EmbeddingIndex = "idx_article_vectors_embedding";
        public const string EmbeddingIndexMethod = "hnsw";
        public const string EmbeddingIndexOperators = "vector_cosine_ops";
        public const string ArticleForeignKey = "knowledge_article_vectors_article_id_fkey";
    }

    public static class Cabinet
    {
        public const string Table = "cabinets";
        public const string Number = "number";
        public const string Description = "description";
        public const string PrimaryKey = "cabinets_pkey";
        public const string NumberUniqueIndex = "cabinets_number_key";
    }

    public static class Software
    {
        public const string Table = "software";
        public const string Name = "name";
        public const string Version = "version";
        public const string PrimaryKey = "software_pkey";
        public const string NameVersionUniqueIndex = "uq_software_name_version";
    }
}
