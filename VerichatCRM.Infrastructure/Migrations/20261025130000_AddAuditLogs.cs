using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using VerichatCRM.Infrastructure.Data;

#nullable disable

namespace VerichatCRM.Infrastructure.Migrations;

[DbContext(typeof(AppDbContext))]
[Migration("20261025130000_AddAuditLogs")]
public partial class AddAuditLogs : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "AuditLogs",
            columns: table => new
            {
                AuditId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                EntityType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                EntityId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                Action = table.Column<string>(type: "nvarchar(max)", nullable: false),
                UserId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                UserEmail = table.Column<string>(type: "nvarchar(max)", nullable: true),
                OldValues = table.Column<string>(type: "nvarchar(max)", nullable: true),
                NewValues = table.Column<string>(type: "nvarchar(max)", nullable: true),
                IpAddress = table.Column<string>(type: "nvarchar(max)", nullable: true),
                Timestamp = table.Column<DateTime>(type: "datetime2", nullable: false)
            },
            constraints: table => table.PrimaryKey("PK_AuditLogs", x => x.AuditId));

    }

    protected override void Down(MigrationBuilder migrationBuilder) =>
        migrationBuilder.DropTable(name: "AuditLogs");
}
