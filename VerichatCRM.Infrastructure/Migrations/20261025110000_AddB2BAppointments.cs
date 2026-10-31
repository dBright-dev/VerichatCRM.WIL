using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using VerichatCRM.Infrastructure.Data;

#nullable disable

namespace VerichatCRM.Infrastructure.Migrations;

[DbContext(typeof(AppDbContext))]
[Migration("20261025110000_AddB2BAppointments")]
public partial class AddB2BAppointments : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "Appointments",
            columns: table => new
            {
                Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                ClientId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                RequestedByUserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                Topic = table.Column<string>(type: "nvarchar(160)", maxLength: 160, nullable: false),
                ScheduledDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                Status = table.Column<string>(type: "nvarchar(24)", maxLength: 24, nullable: false),
                Notes = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                AssignedToUserId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
            },
            constraints: table => table.PrimaryKey("PK_Appointments", x => x.Id));
        migrationBuilder.CreateIndex("IX_Appointments_ClientId_ScheduledDate", "Appointments", new[] { "ClientId", "ScheduledDate" });
        migrationBuilder.CreateIndex("IX_Appointments_Status_ScheduledDate", "Appointments", new[] { "Status", "ScheduledDate" });
    }

    protected override void Down(MigrationBuilder migrationBuilder) => migrationBuilder.DropTable("Appointments");
}
