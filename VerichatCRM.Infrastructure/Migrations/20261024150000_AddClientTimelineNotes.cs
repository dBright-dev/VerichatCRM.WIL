using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using VerichatCRM.Infrastructure.Data;

#nullable disable

namespace VerichatCRM.Infrastructure.Migrations;

[DbContext(typeof(AppDbContext))]
[Migration("20261024150000_AddClientTimelineNotes")]
public partial class AddClientTimelineNotes : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "ClientTimelineNotes",
            columns: table => new
            {
                NoteId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                ClientId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                Text = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: false),
                CreatedByUserId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
            },
            constraints: table => table.PrimaryKey("PK_ClientTimelineNotes", x => x.NoteId));

        migrationBuilder.CreateIndex(
            name: "IX_ClientTimelineNotes_ClientId_CreatedAt",
            table: "ClientTimelineNotes",
            columns: new[] { "ClientId", "CreatedAt" });
    }

    protected override void Down(MigrationBuilder migrationBuilder) =>
        migrationBuilder.DropTable(name: "ClientTimelineNotes");
}
