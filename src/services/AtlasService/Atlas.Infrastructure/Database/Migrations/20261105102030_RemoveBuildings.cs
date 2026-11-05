using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Atlas.Infrastructure.Database.Migrations
{
    /// <inheritdoc />
    public partial class RemoveBuildings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "offices_building_id_fkey",
                schema: "atlas",
                table: "offices");

            migrationBuilder.Sql("ALTER TABLE atlas.offices DROP CONSTRAINT IF EXISTS uq_offices_building_room;");
            migrationBuilder.Sql("DROP INDEX IF EXISTS atlas.uq_offices_building_room;");

            migrationBuilder.DropColumn(
                name: "building_id",
                schema: "atlas",
                table: "offices");

            migrationBuilder.DropTable(
                name: "buildings",
                schema: "atlas");

            migrationBuilder.CreateIndex(
                name: "offices_room_number_key",
                schema: "atlas",
                table: "offices",
                column: "room_number",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "offices_room_number_key",
                schema: "atlas",
                table: "offices");

            migrationBuilder.AddColumn<Guid>(
                name: "building_id",
                schema: "atlas",
                table: "offices",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateTable(
                name: "buildings",
                schema: "atlas",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    address = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("buildings_pkey", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "uq_offices_building_room",
                schema: "atlas",
                table: "offices",
                columns: new[] { "building_id", "room_number" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "offices_building_id_fkey",
                schema: "atlas",
                table: "offices",
                column: "building_id",
                principalSchema: "atlas",
                principalTable: "buildings",
                principalColumn: "id");
        }
    }
}
