using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Pandora.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddAuditTelegramUserId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_audit_user_id",
                table: "audit");

            migrationBuilder.DropColumn(
                name: "user_id",
                table: "audit");

            migrationBuilder.AddColumn<long>(
                name: "telegram_user_id",
                table: "audit",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.CreateIndex(
                name: "IX_audit_telegram_user_id",
                table: "audit",
                column: "telegram_user_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_audit_telegram_user_id",
                table: "audit");

            migrationBuilder.DropColumn(
                name: "telegram_user_id",
                table: "audit");

            migrationBuilder.AddColumn<Guid>(
                name: "user_id",
                table: "audit",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_audit_user_id",
                table: "audit",
                column: "user_id");
        }
    }
}
