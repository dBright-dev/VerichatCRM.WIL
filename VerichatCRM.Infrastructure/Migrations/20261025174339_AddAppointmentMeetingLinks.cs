using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VerichatCRM.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddAppointmentMeetingLinks : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "MeetingUrl",
                table: "Appointments",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MeetingUrl",
                table: "Appointments");
        }
    }
}
