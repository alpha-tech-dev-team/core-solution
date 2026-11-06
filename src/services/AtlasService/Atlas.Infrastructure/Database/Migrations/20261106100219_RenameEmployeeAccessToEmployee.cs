using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Atlas.Infrastructure.Database.Migrations
{
    /// <inheritdoc />
    public partial class RenameEmployeeAccessToEmployee : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "incident_status_history_changed_by_fkey",
                schema: "atlas",
                table: "incident_status_history");

            migrationBuilder.DropForeignKey(
                name: "incidents_assigned_engineer_id_fkey",
                schema: "atlas",
                table: "incidents");

            migrationBuilder.DropForeignKey(
                name: "incidents_employee_id_fkey",
                schema: "atlas",
                table: "incidents");

            migrationBuilder.DropForeignKey(
                name: "knowledge_articles_created_by_fkey",
                schema: "atlas",
                table: "knowledge_articles");

            migrationBuilder.RenameTable(
                name: "employee_access",
                schema: "atlas",
                newName: "employees");

            migrationBuilder.DropPrimaryKey(
                name: "employee_access_pkey",
                schema: "atlas",
                table: "employees");

            migrationBuilder.AddPrimaryKey(
                name: "employees_pkey",
                schema: "atlas",
                table: "employees",
                column: "employee_id");

            migrationBuilder.DropCheckConstraint(
                name: "chk_employee_access_role",
                schema: "atlas",
                table: "employees");

            migrationBuilder.AddCheckConstraint(
                name: "chk_employee_role",
                schema: "atlas",
                table: "employees",
                sql: "role IN ('employee', 'engineer', 'admin')");

            migrationBuilder.RenameIndex(
                name: "employee_access_telegram_id_key",
                schema: "atlas",
                table: "employees",
                newName: "employees_telegram_id_key");

            migrationBuilder.AddForeignKey(
                name: "incident_status_history_changed_by_fkey",
                schema: "atlas",
                table: "incident_status_history",
                column: "changed_by",
                principalSchema: "atlas",
                principalTable: "employees",
                principalColumn: "employee_id");

            migrationBuilder.AddForeignKey(
                name: "incidents_assigned_engineer_id_fkey",
                schema: "atlas",
                table: "incidents",
                column: "assigned_engineer_id",
                principalSchema: "atlas",
                principalTable: "employees",
                principalColumn: "employee_id");

            migrationBuilder.AddForeignKey(
                name: "incidents_employee_id_fkey",
                schema: "atlas",
                table: "incidents",
                column: "employee_id",
                principalSchema: "atlas",
                principalTable: "employees",
                principalColumn: "employee_id");

            migrationBuilder.AddForeignKey(
                name: "knowledge_articles_created_by_fkey",
                schema: "atlas",
                table: "knowledge_articles",
                column: "created_by",
                principalSchema: "atlas",
                principalTable: "employees",
                principalColumn: "employee_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "incident_status_history_changed_by_fkey",
                schema: "atlas",
                table: "incident_status_history");

            migrationBuilder.DropForeignKey(
                name: "incidents_assigned_engineer_id_fkey",
                schema: "atlas",
                table: "incidents");

            migrationBuilder.DropForeignKey(
                name: "incidents_employee_id_fkey",
                schema: "atlas",
                table: "incidents");

            migrationBuilder.DropForeignKey(
                name: "knowledge_articles_created_by_fkey",
                schema: "atlas",
                table: "knowledge_articles");

            migrationBuilder.DropPrimaryKey(
                name: "employees_pkey",
                schema: "atlas",
                table: "employees");

            migrationBuilder.AddPrimaryKey(
                name: "employee_access_pkey",
                schema: "atlas",
                table: "employees",
                column: "employee_id");

            migrationBuilder.DropCheckConstraint(
                name: "chk_employee_role",
                schema: "atlas",
                table: "employees");

            migrationBuilder.AddCheckConstraint(
                name: "chk_employee_access_role",
                schema: "atlas",
                table: "employees",
                sql: "role IN ('employee', 'engineer', 'admin')");

            migrationBuilder.RenameIndex(
                name: "employees_telegram_id_key",
                schema: "atlas",
                table: "employees",
                newName: "employee_access_telegram_id_key");

            migrationBuilder.RenameTable(
                name: "employees",
                schema: "atlas",
                newName: "employee_access");

            migrationBuilder.AddForeignKey(
                name: "incident_status_history_changed_by_fkey",
                schema: "atlas",
                table: "incident_status_history",
                column: "changed_by",
                principalSchema: "atlas",
                principalTable: "employee_access",
                principalColumn: "employee_id");

            migrationBuilder.AddForeignKey(
                name: "incidents_assigned_engineer_id_fkey",
                schema: "atlas",
                table: "incidents",
                column: "assigned_engineer_id",
                principalSchema: "atlas",
                principalTable: "employee_access",
                principalColumn: "employee_id");

            migrationBuilder.AddForeignKey(
                name: "incidents_employee_id_fkey",
                schema: "atlas",
                table: "incidents",
                column: "employee_id",
                principalSchema: "atlas",
                principalTable: "employee_access",
                principalColumn: "employee_id");

            migrationBuilder.AddForeignKey(
                name: "knowledge_articles_created_by_fkey",
                schema: "atlas",
                table: "knowledge_articles",
                column: "created_by",
                principalSchema: "atlas",
                principalTable: "employee_access",
                principalColumn: "employee_id");
        }
    }
}
