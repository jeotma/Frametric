using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Frametric.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class MakeWatchedMovieImportHistoryOptional : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_WatchedMovies_ImportHistories_ImportHistoryId",
                table: "WatchedMovies");

            migrationBuilder.AlterColumn<Guid>(
                name: "ImportHistoryId",
                table: "WatchedMovies",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AddForeignKey(
                name: "FK_WatchedMovies_ImportHistories_ImportHistoryId",
                table: "WatchedMovies",
                column: "ImportHistoryId",
                principalTable: "ImportHistories",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_WatchedMovies_ImportHistories_ImportHistoryId",
                table: "WatchedMovies");

            migrationBuilder.AlterColumn<Guid>(
                name: "ImportHistoryId",
                table: "WatchedMovies",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_WatchedMovies_ImportHistories_ImportHistoryId",
                table: "WatchedMovies",
                column: "ImportHistoryId",
                principalTable: "ImportHistories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
