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
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
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
