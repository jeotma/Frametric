using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Frametric.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCustomListItemNickname : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Nickname",
                table: "CustomListItems",
                type: "character varying(255)",
                maxLength: 255,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Nickname",
                table: "CustomListItems");
        }
    }
}
