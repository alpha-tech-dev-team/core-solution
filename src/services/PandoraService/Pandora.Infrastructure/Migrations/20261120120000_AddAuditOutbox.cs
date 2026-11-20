using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Pandora.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddAuditOutbox : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "audit_outbox",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    payload = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_audit_outbox", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_audit_outbox_created_at",
                table: "audit_outbox",
                column: "created_at");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "audit_outbox");
        }
    }
}
