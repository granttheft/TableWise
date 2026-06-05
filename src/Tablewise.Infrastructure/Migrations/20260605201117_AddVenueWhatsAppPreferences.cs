using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Tablewise.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddVenueWhatsAppPreferences : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "WaNotifyCancellation",
                table: "Venues",
                type: "boolean",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<bool>(
                name: "WaNotifyReminder",
                table: "Venues",
                type: "boolean",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<bool>(
                name: "WaNotifyReservationConfirmed",
                table: "Venues",
                type: "boolean",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<bool>(
                name: "WaNotifyReservationReceived",
                table: "Venues",
                type: "boolean",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<bool>(
                name: "WhatsAppConsent",
                table: "Reservations",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "WaNotifyCancellation",
                table: "Venues");

            migrationBuilder.DropColumn(
                name: "WaNotifyReminder",
                table: "Venues");

            migrationBuilder.DropColumn(
                name: "WaNotifyReservationConfirmed",
                table: "Venues");

            migrationBuilder.DropColumn(
                name: "WaNotifyReservationReceived",
                table: "Venues");

            migrationBuilder.DropColumn(
                name: "WhatsAppConsent",
                table: "Reservations");
        }
    }
}
