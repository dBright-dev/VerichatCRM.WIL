-- ============================================================================
-- Read-only state check for the hosted database on site103828 (api-site.runasp.net).
--
-- Run it BEFORE the six migration files to record the starting point, and AFTER
-- them to prove the end point. It contains no GO separators and writes nothing,
-- so it is safe to paste into a hosting panel web SQL window at any time.
--
-- Expected before: AppliedCount 15, HighestRecorded ...AddAppointmentMeetingLinks,
--                  the six PENDING rows, StaffAccountsGoingToPending = however many
--                  staff accounts exist.
-- Expected after : AppliedCount 21, HighestRecorded ...ReviewSelfClaimedStaffAccounts,
--                  no PENDING rows, ApprovalStatus 'Pending' = the accounts that were
--                  just swept.
-- ============================================================================
SET NOCOUNT ON;

SELECT COUNT(*) AS AppliedCount FROM [__EFMigrationsHistory];
SELECT MAX(MigrationId) AS HighestRecorded FROM [__EFMigrationsHistory];

-- The six files in Database/hosted-migrations, in the order they must run.
SELECT m.MigrationId,
       CASE WHEN h.MigrationId IS NULL THEN N'PENDING' ELSE N'applied' END AS State
FROM (VALUES
    (N'20261027100000_AddUserDeviceToken'),
    (N'20261030204733_AddTask3Features'),
    (N'20261031160000_AddInquiryScoringInputs'),
    (N'20261031210000_AddTicketActivityAndDeliveryStatusHistory'),
    (N'20261101151838_SyncSchemaAndAccountLifecycle'),
    (N'20261101152359_ReviewSelfClaimedStaffAccounts')
) AS m (MigrationId)
LEFT JOIN [__EFMigrationsHistory] AS h ON h.MigrationId = m.MigrationId
ORDER BY m.MigrationId;

-- Which table does the login screen read for a staff account? ReviewSelfClaimed
-- moves four roles to Pending, so this is the list of people who cannot sign in
-- until an administrator approves them. Run it before, and again after.
IF COL_LENGTH(N'[Users]', N'ApprovalStatus') IS NULL
    PRINT N'ApprovalStatus does not exist yet - this is the pre-migration state.';
ELSE
    SELECT Role, ApprovalStatus, COUNT(*) AS Accounts
    FROM [Users]
    GROUP BY Role, ApprovalStatus
    ORDER BY Role, ApprovalStatus;
