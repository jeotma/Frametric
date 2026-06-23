using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Frametric.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddTmdbCollectionFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "TmdbCollectionId",
                table: "Movies",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TmdbCollectionName",
                table: "Movies",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Movies_TmdbCollectionId",
                table: "Movies",
                column: "TmdbCollectionId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Movies_TmdbCollectionId",
                table: "Movies");

            migrationBuilder.DropColumn(
                name: "TmdbCollectionId",
                table: "Movies");

            migrationBuilder.DropColumn(
                name: "TmdbCollectionName",
                table: "Movies");
        }
    }
}
