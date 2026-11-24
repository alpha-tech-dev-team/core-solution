using Atlas.Domain.Enums;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Atlas.Infrastructure.Database.Migrations
{
    /// <inheritdoc />
    public partial class AddIncidentCategoryTypeAndEmployeeTelegramUserId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:Enum:employee_role", "employee,engineer,admin")
                .Annotation("Npgsql:Enum:incident_category_type", "hardware,software,other")
                .Annotation("Npgsql:Enum:incident_priority", "low,medium,high,critical")
                .Annotation("Npgsql:Enum:incident_status", "created,in_progress,resolved,resolved_automatically,closed,cancelled")
                .Annotation("Npgsql:Enum:knowledge_article_status", "draft,published,archived")
                .Annotation("Npgsql:PostgresExtension:vector", ",,")
                .OldAnnotation("Npgsql:Enum:employee_role", "employee,engineer,admin")
                .OldAnnotation("Npgsql:Enum:incident_priority", "low,medium,high,critical")
                .OldAnnotation("Npgsql:Enum:incident_status", "created,in_progress,resolved,resolved_automatically,closed,cancelled")
                .OldAnnotation("Npgsql:Enum:knowledge_article_status", "draft,published,archived")
                .OldAnnotation("Npgsql:PostgresExtension:vector", ",,");

            migrationBuilder.AddColumn<IncidentCategoryType>(
                name: "type",
                table: "incident_categories",
                type: "incident_category_type",
                nullable: false,
                defaultValue: IncidentCategoryType.Other);

            migrationBuilder.Sql(
                """
                UPDATE incident_categories SET type = 'hardware' WHERE id = '0f8fad5b-d9cb-469f-a165-70867728950e';
                UPDATE incident_categories SET type = 'software' WHERE id = '7c9e6679-7425-40de-944b-e07fc1f90ae7';
                UPDATE incident_categories SET type = 'other' WHERE id = 'f47ac10b-58cc-4372-a567-0e02b2c3d479';
                """);

            migrationBuilder.AddColumn<long>(
                name: "telegram_user_id",
                table: "employees",
                type: "bigint",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_employees_telegram_user_id",
                table: "employees",
                column: "telegram_user_id",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_employees_telegram_user_id",
                table: "employees");

            migrationBuilder.DropColumn(
                name: "type",
                table: "incident_categories");

            migrationBuilder.DropColumn(
                name: "telegram_user_id",
                table: "employees");

            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:Enum:employee_role", "employee,engineer,admin")
                .Annotation("Npgsql:Enum:incident_priority", "low,medium,high,critical")
                .Annotation("Npgsql:Enum:incident_status", "created,in_progress,resolved,resolved_automatically,closed,cancelled")
                .Annotation("Npgsql:Enum:knowledge_article_status", "draft,published,archived")
                .Annotation("Npgsql:PostgresExtension:vector", ",,")
                .OldAnnotation("Npgsql:Enum:employee_role", "employee,engineer,admin")
                .OldAnnotation("Npgsql:Enum:incident_category_type", "hardware,software,other")
                .OldAnnotation("Npgsql:Enum:incident_priority", "low,medium,high,critical")
                .OldAnnotation("Npgsql:Enum:incident_status", "created,in_progress,resolved,resolved_automatically,closed,cancelled")
                .OldAnnotation("Npgsql:Enum:knowledge_article_status", "draft,published,archived")
                .OldAnnotation("Npgsql:PostgresExtension:vector", ",,");
        }
    }
}
