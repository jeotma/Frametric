using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Frametric.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddTmdbAndOmdbRatings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "CustomAverageRating",
                table: "Movies",
                type: "double precision",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "ImdbRating",
                table: "Movies",
                type: "double precision",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "MetacriticRating",
                table: "Movies",
                type: "double precision",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "RottenTomatoesRating",
                table: "Movies",
                type: "double precision",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "TmdbPopularity",
                table: "Movies",
                type: "double precision",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "TmdbRating",
                table: "Movies",
                type: "double precision",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CustomAverageRating",
                table: "Movies");

            migrationBuilder.DropColumn(
                name: "ImdbRating",
                table: "Movies");

            migrationBuilder.DropColumn(
                name: "MetacriticRating",
                table: "Movies");

            migrationBuilder.DropColumn(
                name: "RottenTomatoesRating",
                table: "Movies");

            migrationBuilder.DropColumn(
                name: "TmdbPopularity",
                table: "Movies");

            migrationBuilder.DropColumn(
                name: "TmdbRating",
                table: "Movies");
        }
    }
}
