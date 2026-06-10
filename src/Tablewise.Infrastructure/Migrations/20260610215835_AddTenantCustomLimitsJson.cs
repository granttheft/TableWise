using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Tablewise.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddTenantCustomLimitsJson : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CustomLimitsJson",
                table: "Tenants",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CustomLimitsJson",
                table: "Tenants");
        }
    }
}
