using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VerichatCRM.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddMobileDeviceExternalId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ExternalId",
                table: "MobileDevices",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ExternalId",
                table: "MobileDevices");
        }
    }
}
