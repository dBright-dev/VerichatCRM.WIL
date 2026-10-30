using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using VerichatCRM.Infrastructure.Data;

#nullable disable

namespace VerichatCRM.Infrastructure.Migrations;

[DbContext(typeof(AppDbContext))]
[Migration("20261025111000_AddSalesGoals")]
public partial class AddSalesGoals : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder) => migrationBuilder.CreateTable(
        name: "SalesGoals",
        columns: table => new
        {
            Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
            UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
            Title = table.Column<string>(type: "nvarchar(160)", maxLength: 160, nullable: false),
            Target = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
            Achieved = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
            Period = table.Column<string>(type: "nvarchar(24)", maxLength: 24, nullable: false),
            PeriodLabel = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
            CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
            UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
        }, constraints: table => table.PrimaryKey("PK_SalesGoals", x => x.Id));

    protected override void Down(MigrationBuilder migrationBuilder) => migrationBuilder.DropTable("SalesGoals");
}
