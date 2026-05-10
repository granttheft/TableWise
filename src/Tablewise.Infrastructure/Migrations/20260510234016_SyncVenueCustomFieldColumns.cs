using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Tablewise.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class SyncVenueCustomFieldColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsPublic",
                table: "VenueCustomFields",
                type: "boolean",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "VenueCustomFields",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Placeholder",
                table: "VenueCustomFields",
                type: "text",
                nullable: true);

            migrationBuilder.Sql(
                """UPDATE "VenueCustomFields" SET "Name" = "Label" WHERE "Name" = '';""");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsPublic",
                table: "VenueCustomFields");

            migrationBuilder.DropColumn(
                name: "Name",
                table: "VenueCustomFields");

            migrationBuilder.DropColumn(
                name: "Placeholder",
                table: "VenueCustomFields");
        }
    }
}
