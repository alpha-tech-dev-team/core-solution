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
            migrationBuilder.AddColumn<int>(
                name: "type",
                table: "incident_categories",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.Sql(
                """
                UPDATE incident_categories SET type = 0 WHERE id = '0f8fad5b-d9cb-469f-a165-70867728950e';
                UPDATE incident_categories SET type = 1 WHERE id = '7c9e6679-7425-40de-944b-e07fc1f90ae7';
                UPDATE incident_categories SET type = 2 WHERE id = 'f47ac10b-58cc-4372-a567-0e02b2c3d479';
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
        }
    }
}
