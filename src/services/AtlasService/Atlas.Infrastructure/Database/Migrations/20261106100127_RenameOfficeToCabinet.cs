using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Atlas.Infrastructure.Database.Migrations
{
    /// <inheritdoc />
    public partial class RenameOfficeToCabinet : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "hardware_office_id_fkey",
                schema: "atlas",
                table: "hardware");

            migrationBuilder.DropForeignKey(
                name: "incidents_office_id_fkey",
                schema: "atlas",
                table: "incidents");

            migrationBuilder.RenameTable(
                name: "offices",
                schema: "atlas",
                newName: "cabinets");

            migrationBuilder.RenameColumn(
                name: "room_number",
                schema: "atlas",
                table: "cabinets",
                newName: "number");

            migrationBuilder.RenameColumn(
                name: "office_id",
                schema: "atlas",
                table: "hardware",
                newName: "cabinet_id");

            migrationBuilder.RenameColumn(
                name: "office_id",
                schema: "atlas",
                table: "incidents",
                newName: "cabinet_id");

            migrationBuilder.RenameIndex(
                name: "idx_hardware_office_type",
                schema: "atlas",
                table: "hardware",
                newName: "idx_hardware_cabinet_type");

            migrationBuilder.RenameIndex(
                name: "offices_room_number_key",
                schema: "atlas",
                table: "cabinets",
                newName: "cabinets_number_key");

            migrationBuilder.RenameIndex(
                name: "IX_incidents_office_id",
                schema: "atlas",
                table: "incidents",
                newName: "IX_incidents_cabinet_id");

            migrationBuilder.DropPrimaryKey(
                name: "offices_pkey",
                schema: "atlas",
                table: "cabinets");

            migrationBuilder.AddPrimaryKey(
                name: "cabinets_pkey",
                schema: "atlas",
                table: "cabinets",
                column: "id");

            migrationBuilder.AlterColumn<Guid>(
                name: "cabinet_id",
                schema: "atlas",
                table: "hardware",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "hardware_cabinet_id_fkey",
                schema: "atlas",
                table: "hardware",
                column: "cabinet_id",
                principalSchema: "atlas",
                principalTable: "cabinets",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "incidents_cabinet_id_fkey",
                schema: "atlas",
                table: "incidents",
                column: "cabinet_id",
                principalSchema: "atlas",
                principalTable: "cabinets",
                principalColumn: "id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "hardware_cabinet_id_fkey",
                schema: "atlas",
                table: "hardware");

            migrationBuilder.DropForeignKey(
                name: "incidents_cabinet_id_fkey",
                schema: "atlas",
                table: "incidents");

            migrationBuilder.DropPrimaryKey(
                name: "cabinets_pkey",
                schema: "atlas",
                table: "cabinets");

            migrationBuilder.AddPrimaryKey(
                name: "offices_pkey",
                schema: "atlas",
                table: "cabinets",
                column: "id");

            migrationBuilder.RenameIndex(
                name: "idx_hardware_cabinet_type",
                schema: "atlas",
                table: "hardware",
                newName: "idx_hardware_office_type");

            migrationBuilder.RenameIndex(
                name: "cabinets_number_key",
                schema: "atlas",
                table: "cabinets",
                newName: "offices_room_number_key");

            migrationBuilder.RenameIndex(
                name: "IX_incidents_cabinet_id",
                schema: "atlas",
                table: "incidents",
                newName: "IX_incidents_office_id");

            migrationBuilder.RenameColumn(
                name: "number",
                schema: "atlas",
                table: "cabinets",
                newName: "room_number");

            migrationBuilder.RenameColumn(
                name: "cabinet_id",
                schema: "atlas",
                table: "hardware",
                newName: "office_id");

            migrationBuilder.RenameColumn(
                name: "cabinet_id",
                schema: "atlas",
                table: "incidents",
                newName: "office_id");

            migrationBuilder.AlterColumn<Guid>(
                name: "office_id",
                schema: "atlas",
                table: "hardware",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: false);

            migrationBuilder.RenameTable(
                name: "cabinets",
                schema: "atlas",
                newName: "offices");

            migrationBuilder.AddForeignKey(
                name: "hardware_office_id_fkey",
                schema: "atlas",
                table: "hardware",
                column: "office_id",
                principalSchema: "atlas",
                principalTable: "offices",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "incidents_office_id_fkey",
                schema: "atlas",
                table: "incidents",
                column: "office_id",
                principalSchema: "atlas",
                principalTable: "offices",
                principalColumn: "id");
        }
    }
}
