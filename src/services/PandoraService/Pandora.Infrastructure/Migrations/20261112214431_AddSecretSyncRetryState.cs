using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Pandora.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddSecretSyncRetryState : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "attempt_count",
                table: "secret_metadata",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "last_error",
                table: "secret_metadata",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "next_attempt_at",
                table: "secret_metadata",
                type: "timestamp with time zone",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "attempt_count",
                table: "secret_metadata");

            migrationBuilder.DropColumn(
                name: "last_error",
                table: "secret_metadata");

            migrationBuilder.DropColumn(
                name: "next_attempt_at",
                table: "secret_metadata");
        }
    }
}
