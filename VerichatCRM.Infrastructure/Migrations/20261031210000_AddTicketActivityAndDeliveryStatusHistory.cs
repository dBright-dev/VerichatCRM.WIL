using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using VerichatCRM.Infrastructure.Data;

#nullable disable

namespace VerichatCRM.Infrastructure.Migrations;

[DbContext(typeof(AppDbContext))]
[Migration("20261031210000_AddTicketActivityAndDeliveryStatusHistory")]
public partial class AddTicketActivityAndDeliveryStatusHistory : Migration
{
    // Two tables the model has declared for as long as the ticket timeline and the
    // delivery tracking screens have existed, and which no migration in this chain
    // ever created. They cannot be scaffolded into being either: the model snapshot
    // already contains both entities, so the differ sees no change and stays silent,
    // and the absence only shows up as a runtime SqlException -
    //   Invalid object name 'TicketActivity'
    //   Invalid object name 'DeliveryStatusUpdate'
    // - from TicketRepository.AddAsync and the delivery status writer. This file is
    // hand-written for that reason, and it changes nothing except adding these two
    // tables, so it is safe on a database that has data in it.
    //
    // The columns are the snapshot's, verbatim. Both keys are Guid strings, so they
    // are nvarchar(450); everything the model leaves unbounded stays nvarchar(max).

    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "TicketActivity",
            columns: table => new
            {
                ActivityId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                TicketId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                Action = table.Column<string>(type: "nvarchar(max)", nullable: false),
                FromValue = table.Column<string>(type: "nvarchar(max)", nullable: true),
                ToValue = table.Column<string>(type: "nvarchar(max)", nullable: true),
                Notes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                PerformedByUserId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                PerformedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_TicketActivity", x => x.ActivityId);
                table.ForeignKey(
                    name: "FK_TicketActivity_Tickets_TicketId",
                    column: x => x.TicketId,
                    principalTable: "Tickets",
                    principalColumn: "TicketId",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "IX_TicketActivity_TicketId",
            table: "TicketActivity",
            column: "TicketId");

        migrationBuilder.CreateTable(
            name: "DeliveryStatusUpdate",
            columns: table => new
            {
                UpdateId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                DeliveryId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                Location = table.Column<string>(type: "nvarchar(max)", nullable: true),
                Notes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                UpdatedByUserId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                // The shadow foreign key the model builds for DeliveryLog.StatusHistory.
                // It is nullable and the relationship is optional, so deleting a
                // shipment leaves its history rows rather than cascading, which is the
                // behaviour the snapshot declares.
                DeliveryLogDeliveryId = table.Column<string>(type: "nvarchar(450)", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_DeliveryStatusUpdate", x => x.UpdateId);
                table.ForeignKey(
                    name: "FK_DeliveryStatusUpdate_DeliveryLogs_DeliveryLogDeliveryId",
                    column: x => x.DeliveryLogDeliveryId,
                    principalTable: "DeliveryLogs",
                    principalColumn: "DeliveryId",
                    onDelete: ReferentialAction.NoAction);
            });

        migrationBuilder.CreateIndex(
            name: "IX_DeliveryStatusUpdate_DeliveryLogDeliveryId",
            table: "DeliveryStatusUpdate",
            column: "DeliveryLogDeliveryId");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "TicketActivity");
        migrationBuilder.DropTable(name: "DeliveryStatusUpdate");
    }
}
