using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VerichatCRM.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddTask3Features : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "FollowUpStatus",
                table: "Inquiries",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "LastContactDate",
                table: "Inquiries",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "NextFollowUpDate",
                table: "Inquiries",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Sector",
                table: "Clients",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "ContactEmail",
                table: "Clients",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "ClientName",
                table: "Clients",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<double>(
                name: "Latitude",
                table: "Clients",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "Longitude",
                table: "Clients",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<bool>(
                name: "RequiresCalibration",
                table: "Clients",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "SectorType",
                table: "Clients",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "CalibrationSchedules",
                columns: table => new
                {
                    ScheduleId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ClientId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    EquipmentId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ScheduledDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    AssignedTechId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CalibrationSchedules", x => x.ScheduleId);
                    table.ForeignKey(
                        name: "FK_CalibrationSchedules_Assets_EquipmentId",
                        column: x => x.EquipmentId,
                        principalTable: "Assets",
                        principalColumn: "AssetId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CalibrationSchedules_Clients_ClientId",
                        column: x => x.ClientId,
                        principalTable: "Clients",
                        principalColumn: "ClientId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CalibrationSchedules_Users_AssignedTechId",
                        column: x => x.AssignedTechId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ClientAssignments",
                columns: table => new
                {
                    ClientAssignmentId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ClientId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    SalesRepId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    AssignedDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClientAssignments", x => x.ClientAssignmentId);
                    table.ForeignKey(
                        name: "FK_ClientAssignments_Clients_ClientId",
                        column: x => x.ClientId,
                        principalTable: "Clients",
                        principalColumn: "ClientId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ClientAssignments_Users_SalesRepId",
                        column: x => x.SalesRepId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                });

            // No SalesGoals table here. This migration was scaffolded while
            // AppDbContext still carried a second SalesGoal mapping (GoalId /
            // TargetAmount / Type / StartDate / EndDate), so its Up() emitted a
            // CREATE TABLE for a shape that is not the entity and for a table that
            // 20261025111000_AddSalesGoals had already created five days earlier.
            // On every database that migration aborts with
            // "There is already an object named 'SalesGoals'", which is why this
            // file had never been applied anywhere. The mapping that produced it is
            // gone from the context, so the table is not this migration's business.

            migrationBuilder.CreateIndex(
                name: "IX_Inquiries_FollowUpStatus",
                table: "Inquiries",
                column: "FollowUpStatus");

            migrationBuilder.CreateIndex(
                name: "IX_Inquiries_NextFollowUpDate",
                table: "Inquiries",
                column: "NextFollowUpDate");

            migrationBuilder.CreateIndex(
                name: "IX_Clients_SectorType",
                table: "Clients",
                column: "SectorType");

            migrationBuilder.CreateIndex(
                name: "IX_CalibrationSchedules_AssignedTechId",
                table: "CalibrationSchedules",
                column: "AssignedTechId");

            migrationBuilder.CreateIndex(
                name: "IX_CalibrationSchedules_ClientId_ScheduledDate",
                table: "CalibrationSchedules",
                columns: new[] { "ClientId", "ScheduledDate" });

            migrationBuilder.CreateIndex(
                name: "IX_CalibrationSchedules_EquipmentId",
                table: "CalibrationSchedules",
                column: "EquipmentId");

            migrationBuilder.CreateIndex(
                name: "IX_CalibrationSchedules_Status",
                table: "CalibrationSchedules",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_ClientAssignments_ClientId_SalesRepId_AssignedDate",
                table: "ClientAssignments",
                columns: new[] { "ClientId", "SalesRepId", "AssignedDate" });

            migrationBuilder.CreateIndex(
                name: "IX_ClientAssignments_SalesRepId",
                table: "ClientAssignments",
                column: "SalesRepId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CalibrationSchedules");

            migrationBuilder.DropTable(
                name: "ClientAssignments");

            // SalesGoals is not dropped: this migration no longer creates it, and
            // rolling back must not destroy the table 20261025111000_AddSalesGoals owns.

            migrationBuilder.DropIndex(
                name: "IX_Inquiries_FollowUpStatus",
                table: "Inquiries");

            migrationBuilder.DropIndex(
                name: "IX_Inquiries_NextFollowUpDate",
                table: "Inquiries");

            migrationBuilder.DropIndex(
                name: "IX_Clients_SectorType",
                table: "Clients");

            migrationBuilder.DropColumn(
                name: "FollowUpStatus",
                table: "Inquiries");

            migrationBuilder.DropColumn(
                name: "LastContactDate",
                table: "Inquiries");

            migrationBuilder.DropColumn(
                name: "NextFollowUpDate",
                table: "Inquiries");

            migrationBuilder.DropColumn(
                name: "Latitude",
                table: "Clients");

            migrationBuilder.DropColumn(
                name: "Longitude",
                table: "Clients");

            migrationBuilder.DropColumn(
                name: "RequiresCalibration",
                table: "Clients");

            migrationBuilder.DropColumn(
                name: "SectorType",
                table: "Clients");

            migrationBuilder.AlterColumn<string>(
                name: "Sector",
                table: "Clients",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "ContactEmail",
                table: "Clients",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(256)",
                oldMaxLength: 256);

            migrationBuilder.AlterColumn<string>(
                name: "ClientName",
                table: "Clients",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(200)",
                oldMaxLength: 200);
        }
    }
}
