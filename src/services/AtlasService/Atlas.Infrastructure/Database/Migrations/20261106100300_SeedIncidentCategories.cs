using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Atlas.Infrastructure.Database.Migrations
{
    /// <inheritdoc />
    public partial class SeedIncidentCategories : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                schema: "atlas",
                table: "incident_categories",
                columns: new[] { "id", "created_at", "description", "is_active", "name", "updated_at" },
                values: new object[,]
                {
                    { new Guid("0f8fad5b-d9cb-469f-a165-70867728950e"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Hardware incidents", true, "hardware", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("7c9e6679-7425-40de-944b-e07fc1f90ae7"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Software incidents", true, "software", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("f47ac10b-58cc-4372-a567-0e02b2c3d479"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Other incidents", true, "other", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                schema: "atlas",
                table: "incident_categories",
                keyColumn: "id",
                keyValue: new Guid("0f8fad5b-d9cb-469f-a165-70867728950e"));

            migrationBuilder.DeleteData(
                schema: "atlas",
                table: "incident_categories",
                keyColumn: "id",
                keyValue: new Guid("7c9e6679-7425-40de-944b-e07fc1f90ae7"));

            migrationBuilder.DeleteData(
                schema: "atlas",
                table: "incident_categories",
                keyColumn: "id",
                keyValue: new Guid("f47ac10b-58cc-4372-a567-0e02b2c3d479"));
        }
    }
}
