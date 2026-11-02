using System;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using VerichatCRM.Infrastructure.Data;

#nullable disable

namespace VerichatCRM.Infrastructure.Migrations
{
    // The [DbContext] + [Migration] pair is what makes a migration discoverable:
    // EF's assembly scans for MigrationAttribute, so when this file was written by
    // hand without it the class compiled, looked sequenced, and was never
    // collected. dbo.HealthMetrics therefore did not exist on any database built
    // by Migrate(), and POST /api/healthmetrics failed with "Invalid object name
    // 'HealthMetrics'". The timestamp keeps it ordered before the migrations that
    // followed it.
    /// <inheritdoc />
    [DbContext(typeof(AppDbContext))]
    [Migration("20261023000000_AddHealthMetricTable")]
    public partial class AddHealthMetricTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "HealthMetrics",
                columns: table => new
                {
                    MetricId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ClientId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DeviceId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MetricType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Value = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Unit = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Timestamp = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HealthMetrics", x => x.MetricId);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "HealthMetrics");
        }
    }
}
