using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VerichatCRM.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RemoveAccountApprovalGate : Migration
    {
        /// <inheritdoc />
        /// <remarks>
        /// The gate is gone from the code, so these four columns and the queue index that
        /// served it go too. What replaces the review is a narrower door: the sign-up form
        /// can only mint the two client-facing roles, and a staff account now requires an
        /// authenticated administrator at /Admin/Users.
        ///
        /// Switching the code off is the whole of the removal, and that is the hazard. The
        /// gate lived in ApprovalStatus, so the moment the reading code stopped consulting
        /// it every account sitting in Pending became live - including the self-claimed
        /// staff roles that migration 21 deliberately parked for a reviewer. So this first
        /// withdraws access from exactly those rows, then drops the columns that recorded
        /// why. An administrator can see them on the users screen, still disabled, and
        /// decide each one on its merits; nothing is deleted, because a person who filled
        /// in the form is not the thing being removed here.
        ///
        /// Approved and Rejected rows need no handling: the first already signed in, and
        /// the second stays out through IsActive, which this migration does not touch.
        /// </remarks>
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Must precede the DropColumn calls: after them there is no Pending left to find.
            migrationBuilder.Sql(
                @"UPDATE [Users]
                  SET [IsActive] = 0
                  WHERE [ApprovalStatus] = N'Pending';");

            migrationBuilder.DropIndex(
                name: "IX_Users_ApprovalStatus_CreatedAt",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "ApprovalStatus",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "RejectionReason",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "ReviewedAt",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "ReviewedByUserId",
                table: "Users");
        }

        /// <inheritdoc />
        /// <remarks>
        /// Rebuilding the columns is honest only about the schema. Every row comes back
        /// with an empty status and the accounts this migration disabled stay disabled: the
        /// decisions a reviewer made are not recoverable from a dropped column, so a
        /// rollback returns the shape of the gate without its memory.
        /// </remarks>
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ApprovalStatus",
                table: "Users",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "RejectionReason",
                table: "Users",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ReviewedAt",
                table: "Users",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ReviewedByUserId",
                table: "Users",
                type: "nvarchar(450)",
                maxLength: 450,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_ApprovalStatus_CreatedAt",
                table: "Users",
                columns: new[] { "ApprovalStatus", "CreatedAt" });
        }
    }
}
