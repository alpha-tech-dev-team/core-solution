using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Atlas.Infrastructure.Database.Migrations
{
    /// <inheritdoc />
    public partial class RenameIncidentArticleForeignKey : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_incidents_article",
                schema: "atlas",
                table: "incidents");

            migrationBuilder.AddForeignKey(
                name: "incidents_article_id_fkey",
                schema: "atlas",
                table: "incidents",
                column: "article_id",
                principalSchema: "atlas",
                principalTable: "knowledge_articles",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "incidents_article_id_fkey",
                schema: "atlas",
                table: "incidents");

            migrationBuilder.AddForeignKey(
                name: "fk_incidents_article",
                schema: "atlas",
                table: "incidents",
                column: "article_id",
                principalSchema: "atlas",
                principalTable: "knowledge_articles",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);
        }
    }
}
