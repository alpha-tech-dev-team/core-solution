using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Pandora.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCredentialAccessMessageClaim : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "claimed_until",
                table: "credential_access_messages",
                type: "timestamp with time zone",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "claimed_until",
                table: "credential_access_messages");
        }
    }
}
