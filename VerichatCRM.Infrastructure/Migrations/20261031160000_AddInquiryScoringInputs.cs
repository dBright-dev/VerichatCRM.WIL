using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using VerichatCRM.Infrastructure.Data;

#nullable disable

namespace VerichatCRM.Infrastructure.Migrations
{
    /// <summary>
    /// The two inputs the automatic enquiry score needs stored alongside the
    /// score (urgency, estimated value) and the index the queue sorts on.
    /// </summary>
    /// <remarks>
    /// Written by hand rather than scaffolded. After the 2026-10-31 merge the
    /// model snapshot in this folder no longer describes a state any database
    /// reached - <c>20261030204733_AddTask3Features</c> recreates the
    /// <c>SalesGoals</c> table that <c>20261025111000_AddSalesGoals</c> already
    /// created, and the two <c>SalesGoal</c> classes it merged into one file give
    /// different primary keys - so <c>dotnet ef migrations add</c> produced a
    /// migration that rebuilt SalesGoals and re-added columns the earlier
    /// hand-written migrations had already added. This file changes only the
    /// three things listed below, and the snapshot was edited to match.
    /// See DEPLOYMENT.md section 8 for the pre-flight checks before running it.
    /// </remarks>
    /// <inheritdoc />
    [DbContext(typeof(AppDbContext))]
    [Migration("20261031160000_AddInquiryScoringInputs")]
    public partial class AddInquiryScoringInputs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // An nvarchar(max) column cannot be indexed, so Status is narrowed
            // first. The values come from InquiryStatuses; the longest is nine.
            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "Inquiries",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<string>(
                name: "Urgency",
                table: "Inquiries",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "EstimatedValue",
                table: "Inquiries",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Inquiries_Status_Score",
                table: "Inquiries",
                columns: new[] { "Status", "Score" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Inquiries_Status_Score",
                table: "Inquiries");

            migrationBuilder.DropColumn(
                name: "Urgency",
                table: "Inquiries");

            migrationBuilder.DropColumn(
                name: "EstimatedValue",
                table: "Inquiries");

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "Inquiries",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(30)",
                oldMaxLength: 30);
        }
    }
}
