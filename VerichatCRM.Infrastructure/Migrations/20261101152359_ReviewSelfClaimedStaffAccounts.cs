using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VerichatCRM.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ReviewSelfClaimedStaffAccounts : Migration
    {
        /// <inheritdoc />
        /// <remarks>
        /// The gate added by SyncSchemaAndAccountLifecycle only protects accounts
        /// created from now on. This puts the accounts that came before it in front of
        /// a reviewer, and it is a migration rather than a runtime sweep so it happens
        /// exactly once, in order, and is recorded in the same history table as the
        /// columns it depends on.
        ///
        /// Only the four roles that open staff screens move to Pending. The sign-up form
        /// never offered System Administrator - that account comes from the deployment's
        /// own configuration or from an administrator who already holds one - and Client
        /// User and Medical Student reach nothing but their own portal, so reviewing
        /// them would cost a login and protect nothing.
        ///
        /// An account created by an administrator for a real employee is swept up here
        /// too, because nothing in the row records who created it. That is the cost of
        /// this migration, and the reviewer's one click pays it.
        /// </remarks>
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                @"UPDATE [Users]
                  SET [ApprovalStatus] = N'Pending'
                  WHERE [Role] IN (N'Sales Representative', N'Procurement Manager',
                                   N'Field Support Engineer', N'Logistics');");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Only the accounts this migration pended are released again. Anything a
            // reviewer has since decided by hand - including a rejection - is left as
            // they left it, because ReviewedByUserId is only ever written by the queue.
            migrationBuilder.Sql(
                @"UPDATE [Users]
                  SET [ApprovalStatus] = N'Approved'
                  WHERE [Role] IN (N'Sales Representative', N'Procurement Manager',
                                   N'Field Support Engineer', N'Logistics')
                    AND [ApprovalStatus] = N'Pending'
                    AND [ReviewedByUserId] IS NULL;");
        }
    }
}
