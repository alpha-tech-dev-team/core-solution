using System;
using Atlas.Domain.Enums;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using Pgvector;

#nullable disable

namespace Atlas.Infrastructure.Database.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:Enum:employee_role", "employee,engineer,admin")
                .Annotation("Npgsql:Enum:incident_priority", "low,medium,high,critical")
                .Annotation("Npgsql:Enum:incident_status", "created,in_progress,resolved,resolved_automatically,closed,cancelled")
                .Annotation("Npgsql:Enum:knowledge_article_status", "draft,published,archived")
                .Annotation("Npgsql:PostgresExtension:vector", ",,");

            migrationBuilder.CreateTable(
                name: "employees",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    role = table.Column<EmployeeRole>(type: "employee_role", nullable: false, defaultValue: EmployeeRole.Employee),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_employees", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "incident_categories",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    description = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_incident_categories", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "rooms",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    description = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_rooms", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "software",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    version = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_software", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "hardware",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    type = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    inventory_number = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    serial_number = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    room_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_hardware", x => x.id);
                    table.ForeignKey(
                        name: "FK_hardware_rooms_room_id",
                        column: x => x.room_id,
                        principalTable: "rooms",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "incident_feedback",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    incident_id = table.Column<Guid>(type: "uuid", nullable: false),
                    article_id = table.Column<Guid>(type: "uuid", nullable: false),
                    was_helpful = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_incident_feedback", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "incident_status_history",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    incident_id = table.Column<Guid>(type: "uuid", nullable: false),
                    status = table.Column<IncidentStatus>(type: "incident_status", nullable: false),
                    last_changed_by = table.Column<long>(type: "bigint", nullable: true),
                    comment = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_incident_status_history", x => x.id);
                    table.ForeignKey(
                        name: "FK_incident_status_history_employees_last_changed_by",
                        column: x => x.last_changed_by,
                        principalTable: "employees",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "incidents",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    ticket_number = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    employee_id = table.Column<long>(type: "bigint", nullable: false),
                    category_id = table.Column<Guid>(type: "uuid", nullable: false),
                    room_id = table.Column<Guid>(type: "uuid", nullable: true),
                    hardware_id = table.Column<Guid>(type: "uuid", nullable: true),
                    software_id = table.Column<Guid>(type: "uuid", nullable: true),
                    user_description = table.Column<string>(type: "text", nullable: false),
                    status = table.Column<IncidentStatus>(type: "incident_status", nullable: false, defaultValue: IncidentStatus.Created),
                    priority = table.Column<IncidentPriority>(type: "incident_priority", nullable: false, defaultValue: IncidentPriority.Medium),
                    assigned_engineer_id = table.Column<long>(type: "bigint", nullable: true),
                    article_id = table.Column<Guid>(type: "uuid", nullable: true),
                    resolution = table.Column<string>(type: "text", nullable: true),
                    resolved_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_incidents", x => x.id);
                    table.ForeignKey(
                        name: "FK_incidents_employees_assigned_engineer_id",
                        column: x => x.assigned_engineer_id,
                        principalTable: "employees",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_incidents_employees_employee_id",
                        column: x => x.employee_id,
                        principalTable: "employees",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_incidents_hardware_hardware_id",
                        column: x => x.hardware_id,
                        principalTable: "hardware",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_incidents_incident_categories_category_id",
                        column: x => x.category_id,
                        principalTable: "incident_categories",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_incidents_rooms_room_id",
                        column: x => x.room_id,
                        principalTable: "rooms",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_incidents_software_software_id",
                        column: x => x.software_id,
                        principalTable: "software",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "knowledge_articles",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    category_id = table.Column<Guid>(type: "uuid", nullable: true),
                    title = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    symptoms = table.Column<string>(type: "text", nullable: true),
                    solution = table.Column<string>(type: "text", nullable: false),
                    status = table.Column<KnowledgeArticleStatus>(type: "knowledge_article_status", nullable: false, defaultValue: KnowledgeArticleStatus.Draft),
                    created_by = table.Column<long>(type: "bigint", nullable: true),
                    source_incident_id = table.Column<Guid>(type: "uuid", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_knowledge_articles", x => x.id);
                    table.ForeignKey(
                        name: "FK_knowledge_articles_employees_created_by",
                        column: x => x.created_by,
                        principalTable: "employees",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_knowledge_articles_incident_categories_category_id",
                        column: x => x.category_id,
                        principalTable: "incident_categories",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_knowledge_articles_incidents_source_incident_id",
                        column: x => x.source_incident_id,
                        principalTable: "incidents",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "knowledge_article_vectors",
                columns: table => new
                {
                    article_id = table.Column<Guid>(type: "uuid", nullable: false),
                    embedding = table.Column<Vector>(type: "vector(1536)", nullable: false),
                    embedding_model = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    embedded_text = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_knowledge_article_vectors", x => x.article_id);
                    table.ForeignKey(
                        name: "FK_knowledge_article_vectors_knowledge_articles_article_id",
                        column: x => x.article_id,
                        principalTable: "knowledge_articles",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_hardware_inventory_number",
                table: "hardware",
                column: "inventory_number",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_hardware_room_id_type",
                table: "hardware",
                columns: new[] { "room_id", "type" });

            migrationBuilder.CreateIndex(
                name: "IX_incident_categories_name",
                table: "incident_categories",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_incident_feedback_article_id",
                table: "incident_feedback",
                column: "article_id");

            migrationBuilder.CreateIndex(
                name: "IX_incident_feedback_incident_id_article_id",
                table: "incident_feedback",
                columns: new[] { "incident_id", "article_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_incident_status_history_incident_id_created_at",
                table: "incident_status_history",
                columns: new[] { "incident_id", "created_at" });

            migrationBuilder.CreateIndex(
                name: "IX_incident_status_history_last_changed_by",
                table: "incident_status_history",
                column: "last_changed_by");

            migrationBuilder.CreateIndex(
                name: "IX_incidents_article_id",
                table: "incidents",
                column: "article_id");

            migrationBuilder.CreateIndex(
                name: "IX_incidents_assigned_engineer_id",
                table: "incidents",
                column: "assigned_engineer_id");

            migrationBuilder.CreateIndex(
                name: "IX_incidents_category_id",
                table: "incidents",
                column: "category_id");

            migrationBuilder.CreateIndex(
                name: "IX_incidents_created_at",
                table: "incidents",
                column: "created_at");

            migrationBuilder.CreateIndex(
                name: "IX_incidents_employee_id",
                table: "incidents",
                column: "employee_id");

            migrationBuilder.CreateIndex(
                name: "IX_incidents_hardware_id",
                table: "incidents",
                column: "hardware_id");

            migrationBuilder.CreateIndex(
                name: "IX_incidents_room_id",
                table: "incidents",
                column: "room_id");

            migrationBuilder.CreateIndex(
                name: "IX_incidents_software_id",
                table: "incidents",
                column: "software_id");

            migrationBuilder.CreateIndex(
                name: "IX_incidents_status_priority",
                table: "incidents",
                columns: new[] { "status", "priority" });

            migrationBuilder.CreateIndex(
                name: "IX_knowledge_article_vectors_embedding",
                table: "knowledge_article_vectors",
                column: "embedding")
                .Annotation("Npgsql:IndexMethod", "hnsw")
                .Annotation("Npgsql:IndexOperators", new[] { "vector_cosine_ops" });

            migrationBuilder.CreateIndex(
                name: "IX_knowledge_articles_category_id",
                table: "knowledge_articles",
                column: "category_id");

            migrationBuilder.CreateIndex(
                name: "IX_knowledge_articles_created_by",
                table: "knowledge_articles",
                column: "created_by");

            migrationBuilder.CreateIndex(
                name: "IX_knowledge_articles_source_incident_id",
                table: "knowledge_articles",
                column: "source_incident_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_knowledge_articles_status",
                table: "knowledge_articles",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "IX_rooms_name",
                table: "rooms",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_software_name_version",
                table: "software",
                columns: new[] { "name", "version" },
                unique: true)
                .Annotation("Npgsql:NullsDistinct", false);

            migrationBuilder.AddForeignKey(
                name: "FK_incident_feedback_incidents_incident_id",
                table: "incident_feedback",
                column: "incident_id",
                principalTable: "incidents",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_incident_feedback_knowledge_articles_article_id",
                table: "incident_feedback",
                column: "article_id",
                principalTable: "knowledge_articles",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_incident_status_history_incidents_incident_id",
                table: "incident_status_history",
                column: "incident_id",
                principalTable: "incidents",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_incidents_knowledge_articles_article_id",
                table: "incidents",
                column: "article_id",
                principalTable: "knowledge_articles",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_hardware_rooms_room_id",
                table: "hardware");

            migrationBuilder.DropForeignKey(
                name: "FK_incidents_rooms_room_id",
                table: "incidents");

            migrationBuilder.DropForeignKey(
                name: "FK_knowledge_articles_incidents_source_incident_id",
                table: "knowledge_articles");

            migrationBuilder.DropTable(
                name: "incident_feedback");

            migrationBuilder.DropTable(
                name: "incident_status_history");

            migrationBuilder.DropTable(
                name: "knowledge_article_vectors");

            migrationBuilder.DropTable(
                name: "rooms");

            migrationBuilder.DropTable(
                name: "incidents");

            migrationBuilder.DropTable(
                name: "hardware");

            migrationBuilder.DropTable(
                name: "knowledge_articles");

            migrationBuilder.DropTable(
                name: "software");

            migrationBuilder.DropTable(
                name: "employees");

            migrationBuilder.DropTable(
                name: "incident_categories");
        }
    }
}
