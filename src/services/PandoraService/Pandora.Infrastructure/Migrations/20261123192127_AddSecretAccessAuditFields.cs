using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Pandora.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddSecretAccessAuditFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "granted_at",
                table: "permissions",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "granted_by",
                table: "permissions",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "target_telegram_user_id",
                table: "audit",
                type: "bigint",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "granted_at",
                table: "permissions");

            migrationBuilder.DropColumn(
                name: "granted_by",
                table: "permissions");

            migrationBuilder.DropColumn(
                name: "target_telegram_user_id",
                table: "audit");
        }
    }
}
