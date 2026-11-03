using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Pandora.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPermissionTelegramUserId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_permissions",
                table: "permissions");

            migrationBuilder.DropColumn(
                name: "user_id",
                table: "permissions");

            migrationBuilder.AddColumn<long>(
                name: "telegram_user_id",
                table: "permissions",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddPrimaryKey(
                name: "PK_permissions",
                table: "permissions",
                columns: new[] { "telegram_user_id", "vault_item_id" });

            migrationBuilder.InsertData(
                table: "secret_metadata",
                columns: new[] { "vault_item_id", "resource_name", "username" },
                values: new object[,]
                {
                    { "61996eaf-6ec9-4a09-96ee-622d8d28fc2e", "ClickUp", "service@pandora.local" },
                    { "1e1acdc1-c912-4b6d-92a4-5de9faff9ef9", "GitHub", "service@pandora.local" }
                });

            migrationBuilder.InsertData(
                table: "permissions",
                columns: new[] { "vault_item_id", "telegram_user_id", "is_active" },
                values: new object[,]
                {
                    { "61996eaf-6ec9-4a09-96ee-622d8d28fc2e", 705295858L, true },
                    { "1e1acdc1-c912-4b6d-92a4-5de9faff9ef9", 705295858L, true }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "permissions",
                keyColumns: new[] { "telegram_user_id", "vault_item_id" },
                keyValues: new object[,]
                {
                    { 705295858L, "61996eaf-6ec9-4a09-96ee-622d8d28fc2e" },
                    { 705295858L, "1e1acdc1-c912-4b6d-92a4-5de9faff9ef9" }
                });
            migrationBuilder.DeleteData(
                table: "secret_metadata",
                keyColumn: "vault_item_id",
                keyValues: new object[]
                {
                    "61996eaf-6ec9-4a09-96ee-622d8d28fc2e",
                    "1e1acdc1-c912-4b6d-92a4-5de9faff9ef9"
                });

            migrationBuilder.DropPrimaryKey(
                name: "PK_permissions",
                table: "permissions");

            migrationBuilder.DropColumn(
                name: "telegram_user_id",
                table: "permissions");

            migrationBuilder.AddColumn<Guid>(
                name: "user_id",
                table: "permissions",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddPrimaryKey(
                name: "PK_permissions",
                table: "permissions",
                columns: new[] { "user_id", "vault_item_id" });
        }
    }
}
