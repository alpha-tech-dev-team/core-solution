using System;
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
            migrationBuilder.EnsureSchema(
                name: "atlas");

            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:PostgresExtension:vector", ",,");

            migrationBuilder.CreateTable(
                name: "buildings",
                schema: "atlas",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    address = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("buildings_pkey", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "employee_access",
                schema: "atlas",
                columns: table => new
                {
                    employee_id = table.Column<Guid>(type: "uuid", nullable: false),
                    telegram_id = table.Column<long>(type: "bigint", nullable: true),
                    role = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, defaultValue: "employee"),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("employee_access_pkey", x => x.employee_id);
                    table.CheckConstraint("chk_employee_access_role", "role IN ('employee', 'engineer', 'admin')");
                });

            migrationBuilder.CreateTable(
                name: "incident_categories",
                schema: "atlas",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    description = table.Column<string>(type: "text", nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("incident_categories_pkey", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "software",
                schema: "atlas",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    version = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("software_pkey", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "offices",
                schema: "atlas",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    building_id = table.Column<Guid>(type: "uuid", nullable: false),
                    room_number = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    description = table.Column<string>(type: "text", nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("offices_pkey", x => x.id);
                    table.ForeignKey(
                        name: "offices_building_id_fkey",
                        column: x => x.building_id,
                        principalSchema: "atlas",
                        principalTable: "buildings",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "hardware",
                schema: "atlas",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    type = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    inventory_number = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    serial_number = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    office_id = table.Column<Guid>(type: "uuid", nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("hardware_pkey", x => x.id);
                    table.ForeignKey(
                        name: "hardware_office_id_fkey",
                        column: x => x.office_id,
                        principalSchema: "atlas",
                        principalTable: "offices",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "incident_feedback",
                schema: "atlas",
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
                    table.PrimaryKey("incident_feedback_pkey", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "incident_status_history",
                schema: "atlas",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    incident_id = table.Column<Guid>(type: "uuid", nullable: false),
                    old_status = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    new_status = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    changed_by = table.Column<Guid>(type: "uuid", nullable: true),
                    comment = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("incident_status_history_pkey", x => x.id);
                    table.ForeignKey(
                        name: "incident_status_history_changed_by_fkey",
                        column: x => x.changed_by,
                        principalSchema: "atlas",
                        principalTable: "employee_access",
                        principalColumn: "employee_id");
                });

            migrationBuilder.CreateTable(
                name: "incidents",
                schema: "atlas",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    ticket_number = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    employee_id = table.Column<Guid>(type: "uuid", nullable: false),
                    category_id = table.Column<Guid>(type: "uuid", nullable: false),
                    office_id = table.Column<Guid>(type: "uuid", nullable: true),
                    hardware_id = table.Column<Guid>(type: "uuid", nullable: true),
                    software_id = table.Column<Guid>(type: "uuid", nullable: true),
                    user_description = table.Column<string>(type: "text", nullable: false),
                    status = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false, defaultValue: "created"),
                    priority = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, defaultValue: "medium"),
                    assigned_engineer_id = table.Column<Guid>(type: "uuid", nullable: true),
                    article_id = table.Column<Guid>(type: "uuid", nullable: true),
                    resolution = table.Column<string>(type: "text", nullable: true),
                    resolved_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("incidents_pkey", x => x.id);
                    table.CheckConstraint("chk_incident_asset", "hardware_id IS NULL OR software_id IS NULL");
                    table.CheckConstraint("chk_incident_assignee", "status <> 'in_progress' OR assigned_engineer_id IS NOT NULL");
                    table.CheckConstraint("chk_incident_description", "length(trim(user_description)) > 0");
                    table.CheckConstraint("chk_incident_priority", "priority IN ('low', 'medium', 'high', 'critical')");
                    table.CheckConstraint("chk_incident_resolution", "status NOT IN ('resolved', 'resolved_automatically') OR (resolution IS NOT NULL AND length(trim(resolution)) > 0)");
                    table.CheckConstraint("chk_incident_status", "status IN ('created', 'in_progress', 'resolved', 'resolved_automatically', 'closed', 'cancelled')");
                    table.ForeignKey(
                        name: "incidents_assigned_engineer_id_fkey",
                        column: x => x.assigned_engineer_id,
                        principalSchema: "atlas",
                        principalTable: "employee_access",
                        principalColumn: "employee_id");
                    table.ForeignKey(
                        name: "incidents_category_id_fkey",
                        column: x => x.category_id,
                        principalSchema: "atlas",
                        principalTable: "incident_categories",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "incidents_employee_id_fkey",
                        column: x => x.employee_id,
                        principalSchema: "atlas",
                        principalTable: "employee_access",
                        principalColumn: "employee_id");
                    table.ForeignKey(
                        name: "incidents_hardware_id_fkey",
                        column: x => x.hardware_id,
                        principalSchema: "atlas",
                        principalTable: "hardware",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "incidents_office_id_fkey",
                        column: x => x.office_id,
                        principalSchema: "atlas",
                        principalTable: "offices",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "incidents_software_id_fkey",
                        column: x => x.software_id,
                        principalSchema: "atlas",
                        principalTable: "software",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "knowledge_articles",
                schema: "atlas",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    category_id = table.Column<Guid>(type: "uuid", nullable: true),
                    title = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    symptoms = table.Column<string>(type: "text", nullable: true),
                    solution = table.Column<string>(type: "text", nullable: false),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, defaultValue: "draft"),
                    created_by = table.Column<Guid>(type: "uuid", nullable: true),
                    source_incident_id = table.Column<Guid>(type: "uuid", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("knowledge_articles_pkey", x => x.id);
                    table.CheckConstraint("chk_article_solution", "length(trim(solution)) > 0");
                    table.CheckConstraint("chk_article_status", "status IN ('draft', 'published', 'archived')");
                    table.CheckConstraint("chk_article_title", "length(trim(title)) > 0");
                    table.ForeignKey(
                        name: "knowledge_articles_category_id_fkey",
                        column: x => x.category_id,
                        principalSchema: "atlas",
                        principalTable: "incident_categories",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "knowledge_articles_created_by_fkey",
                        column: x => x.created_by,
                        principalSchema: "atlas",
                        principalTable: "employee_access",
                        principalColumn: "employee_id");
                    table.ForeignKey(
                        name: "knowledge_articles_source_incident_id_fkey",
                        column: x => x.source_incident_id,
                        principalSchema: "atlas",
                        principalTable: "incidents",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "knowledge_article_vectors",
                schema: "atlas",
                columns: table => new
                {
                    article_id = table.Column<Guid>(type: "uuid", nullable: false),
                    embedding = table.Column<Vector>(type: "vector(1536)", nullable: false),
                    embedding_model = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    embedded_text = table.Column<string>(type: "text", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("knowledge_article_vectors_pkey", x => x.article_id);
                    table.ForeignKey(
                        name: "knowledge_article_vectors_article_id_fkey",
                        column: x => x.article_id,
                        principalSchema: "atlas",
                        principalTable: "knowledge_articles",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "employee_access_telegram_id_key",
                schema: "atlas",
                table: "employee_access",
                column: "telegram_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "hardware_inventory_number_key",
                schema: "atlas",
                table: "hardware",
                column: "inventory_number",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "idx_hardware_office_type",
                schema: "atlas",
                table: "hardware",
                columns: new[] { "office_id", "type" });

            migrationBuilder.CreateIndex(
                name: "incident_categories_name_key",
                schema: "atlas",
                table: "incident_categories",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "idx_incident_feedback_article",
                schema: "atlas",
                table: "incident_feedback",
                column: "article_id");

            migrationBuilder.CreateIndex(
                name: "uq_incident_feedback",
                schema: "atlas",
                table: "incident_feedback",
                columns: new[] { "incident_id", "article_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "idx_incident_status_history_incident",
                schema: "atlas",
                table: "incident_status_history",
                columns: new[] { "incident_id", "created_at" });

            migrationBuilder.CreateIndex(
                name: "IX_incident_status_history_changed_by",
                schema: "atlas",
                table: "incident_status_history",
                column: "changed_by");

            migrationBuilder.CreateIndex(
                name: "idx_incidents_assigned_engineer",
                schema: "atlas",
                table: "incidents",
                column: "assigned_engineer_id");

            migrationBuilder.CreateIndex(
                name: "idx_incidents_category",
                schema: "atlas",
                table: "incidents",
                column: "category_id");

            migrationBuilder.CreateIndex(
                name: "idx_incidents_created_at",
                schema: "atlas",
                table: "incidents",
                column: "created_at");

            migrationBuilder.CreateIndex(
                name: "idx_incidents_employee",
                schema: "atlas",
                table: "incidents",
                column: "employee_id");

            migrationBuilder.CreateIndex(
                name: "idx_incidents_status_priority",
                schema: "atlas",
                table: "incidents",
                columns: new[] { "status", "priority" });

            migrationBuilder.CreateIndex(
                name: "incidents_ticket_number_key",
                schema: "atlas",
                table: "incidents",
                column: "ticket_number",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_incidents_article_id",
                schema: "atlas",
                table: "incidents",
                column: "article_id");

            migrationBuilder.CreateIndex(
                name: "IX_incidents_hardware_id",
                schema: "atlas",
                table: "incidents",
                column: "hardware_id");

            migrationBuilder.CreateIndex(
                name: "IX_incidents_office_id",
                schema: "atlas",
                table: "incidents",
                column: "office_id");

            migrationBuilder.CreateIndex(
                name: "IX_incidents_software_id",
                schema: "atlas",
                table: "incidents",
                column: "software_id");

            migrationBuilder.CreateIndex(
                name: "idx_article_vectors_embedding",
                schema: "atlas",
                table: "knowledge_article_vectors",
                column: "embedding")
                .Annotation("Npgsql:IndexMethod", "hnsw")
                .Annotation("Npgsql:IndexOperators", new[] { "vector_cosine_ops" });

            migrationBuilder.CreateIndex(
                name: "idx_knowledge_articles_category",
                schema: "atlas",
                table: "knowledge_articles",
                column: "category_id");

            migrationBuilder.CreateIndex(
                name: "idx_knowledge_articles_status",
                schema: "atlas",
                table: "knowledge_articles",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "IX_knowledge_articles_created_by",
                schema: "atlas",
                table: "knowledge_articles",
                column: "created_by");

            migrationBuilder.CreateIndex(
                name: "knowledge_articles_source_incident_id_key",
                schema: "atlas",
                table: "knowledge_articles",
                column: "source_incident_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "uq_offices_building_room",
                schema: "atlas",
                table: "offices",
                columns: new[] { "building_id", "room_number" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "uq_software_name_version",
                schema: "atlas",
                table: "software",
                columns: new[] { "name", "version" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "incident_feedback_article_id_fkey",
                schema: "atlas",
                table: "incident_feedback",
                column: "article_id",
                principalSchema: "atlas",
                principalTable: "knowledge_articles",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "incident_feedback_incident_id_fkey",
                schema: "atlas",
                table: "incident_feedback",
                column: "incident_id",
                principalSchema: "atlas",
                principalTable: "incidents",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "incident_status_history_incident_id_fkey",
                schema: "atlas",
                table: "incident_status_history",
                column: "incident_id",
                principalSchema: "atlas",
                principalTable: "incidents",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_incidents_article",
                schema: "atlas",
                table: "incidents",
                column: "article_id",
                principalSchema: "atlas",
                principalTable: "knowledge_articles",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "hardware_office_id_fkey",
                schema: "atlas",
                table: "hardware");

            migrationBuilder.DropForeignKey(
                name: "incidents_office_id_fkey",
                schema: "atlas",
                table: "incidents");

            migrationBuilder.DropForeignKey(
                name: "fk_incidents_article",
                schema: "atlas",
                table: "incidents");

            migrationBuilder.DropTable(
                name: "incident_feedback",
                schema: "atlas");

            migrationBuilder.DropTable(
                name: "incident_status_history",
                schema: "atlas");

            migrationBuilder.DropTable(
                name: "knowledge_article_vectors",
                schema: "atlas");

            migrationBuilder.DropTable(
                name: "offices",
                schema: "atlas");

            migrationBuilder.DropTable(
                name: "buildings",
                schema: "atlas");

            migrationBuilder.DropTable(
                name: "knowledge_articles",
                schema: "atlas");

            migrationBuilder.DropTable(
                name: "incidents",
                schema: "atlas");

            migrationBuilder.DropTable(
                name: "employee_access",
                schema: "atlas");

            migrationBuilder.DropTable(
                name: "incident_categories",
                schema: "atlas");

            migrationBuilder.DropTable(
                name: "hardware",
                schema: "atlas");

            migrationBuilder.DropTable(
                name: "software",
                schema: "atlas");
        }
    }
}
