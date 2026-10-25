using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VerichatCRM.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreateupgrade : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Role",
                table: "Users",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Email",
                table: "Users",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "DisplayName",
                table: "Users",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.CreateTable(
                name: "Appointments",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ClientId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    RequestedByUserId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Topic = table.Column<string>(type: "nvarchar(160)", maxLength: 160, nullable: false),
                    ScheduledDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(24)", maxLength: 24, nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    AssignedToUserId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Appointments", x => x.Id);
                });

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
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClientTimelineNotes", x => x.NoteId);
                });

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

            migrationBuilder.CreateTable(
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
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SalesGoals", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Appointments_ClientId_ScheduledDate",
                table: "Appointments",
                columns: new[] { "ClientId", "ScheduledDate" });

            migrationBuilder.CreateIndex(
                name: "IX_Appointments_Status_ScheduledDate",
                table: "Appointments",
                columns: new[] { "Status", "ScheduledDate" });

            migrationBuilder.CreateIndex(
                name: "IX_ClientTimelineNotes_ClientId_CreatedAt",
                table: "ClientTimelineNotes",
                columns: new[] { "ClientId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_SalesGoals_UserId_Period",
                table: "SalesGoals",
                columns: new[] { "UserId", "Period" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Appointments");

            migrationBuilder.DropTable(
                name: "ClientTimelineNotes");

            migrationBuilder.DropTable(
                name: "HealthMetrics");

            migrationBuilder.DropTable(
                name: "SalesGoals");

            migrationBuilder.AlterColumn<string>(
                name: "Role",
                table: "Users",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<string>(
                name: "Email",
                table: "Users",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(200)",
                oldMaxLength: 200);

            migrationBuilder.AlterColumn<string>(
                name: "DisplayName",
                table: "Users",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(150)",
                oldMaxLength: 150);
        }
    }
}
