using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Frametric.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Phase2SecurityAndHistory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "ImportHistoryId",
                table: "WatchlistItems",
                type: "uuid",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Username",
                table: "Users",
                type: "character varying(150)",
                maxLength: 150,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "Email",
                table: "Users",
                type: "character varying(255)",
                maxLength: 255,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddColumn<string>(
                name: "PasswordHash",
                table: "Users",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<Guid>(
                name: "ImportHistoryId",
                table: "MovieRatings",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ImportHistoryId",
                table: "MovieLikes",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ImportHistoryId",
                table: "DiaryEntries",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ImportHistories",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    ImportDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    RowCount = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ProviderSource = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ErrorMessage = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ImportHistories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ImportHistories_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RefreshTokens",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Token = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    RevokedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RefreshTokens", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RefreshTokens_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_WatchlistItems_ImportHistoryId",
                table: "WatchlistItems",
                column: "ImportHistoryId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_Email",
                table: "Users",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_Username",
                table: "Users",
                column: "Username",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MovieRatings_ImportHistoryId",
                table: "MovieRatings",
                column: "ImportHistoryId");

            migrationBuilder.CreateIndex(
                name: "IX_MovieLikes_ImportHistoryId",
                table: "MovieLikes",
                column: "ImportHistoryId");

            migrationBuilder.CreateIndex(
                name: "IX_DiaryEntries_ImportHistoryId",
                table: "DiaryEntries",
                column: "ImportHistoryId");

            migrationBuilder.CreateIndex(
                name: "IX_ImportHistories_UserId",
                table: "ImportHistories",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_RefreshTokens_Token",
                table: "RefreshTokens",
                column: "Token",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RefreshTokens_UserId",
                table: "RefreshTokens",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_DiaryEntries_ImportHistories_ImportHistoryId",
                table: "DiaryEntries",
                column: "ImportHistoryId",
                principalTable: "ImportHistories",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_MovieLikes_ImportHistories_ImportHistoryId",
                table: "MovieLikes",
                column: "ImportHistoryId",
                principalTable: "ImportHistories",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_MovieRatings_ImportHistories_ImportHistoryId",
                table: "MovieRatings",
                column: "ImportHistoryId",
                principalTable: "ImportHistories",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_WatchlistItems_ImportHistories_ImportHistoryId",
                table: "WatchlistItems",
                column: "ImportHistoryId",
                principalTable: "ImportHistories",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.CreateIndex(
                name: "IX_DiaryEntries_UserId_WatchedDate",
                table: "DiaryEntries",
                columns: new[] { "UserId", "WatchedDate" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_DiaryEntries_UserId_WatchedDate",
                table: "DiaryEntries");

            migrationBuilder.DropForeignKey(
                name: "FK_DiaryEntries_ImportHistories_ImportHistoryId",
                table: "DiaryEntries");

            migrationBuilder.DropForeignKey(
                name: "FK_MovieLikes_ImportHistories_ImportHistoryId",
                table: "MovieLikes");

            migrationBuilder.DropForeignKey(
                name: "FK_MovieRatings_ImportHistories_ImportHistoryId",
                table: "MovieRatings");

            migrationBuilder.DropForeignKey(
                name: "FK_WatchlistItems_ImportHistories_ImportHistoryId",
                table: "WatchlistItems");

            migrationBuilder.DropTable(
                name: "ImportHistories");

            migrationBuilder.DropTable(
                name: "RefreshTokens");

            migrationBuilder.DropIndex(
                name: "IX_WatchlistItems_ImportHistoryId",
                table: "WatchlistItems");

            migrationBuilder.DropIndex(
                name: "IX_Users_Email",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Users_Username",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_MovieRatings_ImportHistoryId",
                table: "MovieRatings");

            migrationBuilder.DropIndex(
                name: "IX_MovieLikes_ImportHistoryId",
                table: "MovieLikes");

            migrationBuilder.DropIndex(
                name: "IX_DiaryEntries_ImportHistoryId",
                table: "DiaryEntries");

            migrationBuilder.DropColumn(
                name: "ImportHistoryId",
                table: "WatchlistItems");

            migrationBuilder.DropColumn(
                name: "PasswordHash",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "ImportHistoryId",
                table: "MovieRatings");

            migrationBuilder.DropColumn(
                name: "ImportHistoryId",
                table: "MovieLikes");

            migrationBuilder.DropColumn(
                name: "ImportHistoryId",
                table: "DiaryEntries");

            migrationBuilder.AlterColumn<string>(
                name: "Username",
                table: "Users",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(150)",
                oldMaxLength: 150);

            migrationBuilder.AlterColumn<string>(
                name: "Email",
                table: "Users",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(255)",
                oldMaxLength: 255);
        }
    }
}
