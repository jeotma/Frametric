using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Frametric.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddSuperAdminAndRevisions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "CanAddUsers",
                table: "Users",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "CanDeleteUsers",
                table: "Users",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "CanManageCatalog",
                table: "Users",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "CanPromoteToAdmin",
                table: "Users",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "SuperAdminNotificationSent",
                table: "Users",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "EntityRevisions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EntityType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    EntityId = table.Column<Guid>(type: "uuid", nullable: false),
                    ChangedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ChangedBy = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    StateJson = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EntityRevisions", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EntityRevisions_EntityType_EntityId",
                table: "EntityRevisions",
                columns: new[] { "EntityType", "EntityId" });

            // Custom raw SQL trigger for pg_notify on SuperAdmin promotion
            migrationBuilder.Sql(@"
                CREATE OR REPLACE FUNCTION notify_superadmin_promotion()
                RETURNS TRIGGER AS $$
                BEGIN
                    IF NEW.""Role"" = 'SuperAdmin' AND (OLD.""Role"" IS NULL OR OLD.""Role"" <> 'SuperAdmin') THEN
                        PERFORM pg_notify('superadmin_promoted', json_build_object('UserId', NEW.""Id""::text, 'PromotedBy', current_user)::text);
                    END IF;
                    RETURN NEW;
                END;
                $$ LANGUAGE plpgsql;
            ");

            migrationBuilder.Sql(@"
                CREATE OR REPLACE TRIGGER trg_user_superadmin_promoted
                AFTER UPDATE ON ""Users""
                FOR EACH ROW
                EXECUTE FUNCTION notify_superadmin_promotion();
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP TRIGGER IF EXISTS trg_user_superadmin_promoted ON \"Users\";");
            migrationBuilder.Sql("DROP FUNCTION IF EXISTS notify_superadmin_promotion();");

            migrationBuilder.DropTable(
                name: "EntityRevisions");

            migrationBuilder.DropColumn(
                name: "CanAddUsers",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "CanDeleteUsers",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "CanManageCatalog",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "CanPromoteToAdmin",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "SuperAdminNotificationSent",
                table: "Users");
        }
    }
}
