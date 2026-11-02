-- ============================================================================
-- Read-only state check for the hosted database on site103828 (api-site.runasp.net).
--
-- Run it BEFORE the seven migration files to record the starting point, and AFTER
-- them to prove the end point. It contains no GO separators and writes nothing,
-- so it is safe to paste into a hosting panel web SQL window at any time.
--
-- Expected before: AppliedCount 15, HighestRecorded ...AddAppointmentMeetingLinks,
--                  the seven PENDING rows, and whatever ApprovalStatus the accounts
--                  that predate the gate carry.
-- Expected after : AppliedCount 22, HighestRecorded ...RemoveAccountApprovalGate,
--                  no PENDING rows, and no ApprovalStatus column at all - the last
--                  query below prints its absent line, which is the proof that 22
--                  landed rather than merely being recorded.
--
-- The two states the last query can show are deliberately asymmetric. Before 22 it
-- groups the Users table by a column the application has stopped reading, which is
-- how you spot the accounts 22 is about to disable; after 22 that column is gone,
-- and the surviving question - who cannot sign in - is answered by IsActive.
-- ============================================================================
SET NOCOUNT ON;

SELECT COUNT(*) AS AppliedCount FROM [__EFMigrationsHistory];
SELECT MAX(MigrationId) AS HighestRecorded FROM [__EFMigrationsHistory];

-- The seven files in Database/hosted-migrations, in the order they must run.
SELECT m.MigrationId,
       CASE WHEN h.MigrationId IS NULL THEN N'PENDING' ELSE N'applied' END AS State
FROM (VALUES
    (N'20261027100000_AddUserDeviceToken'),
    (N'20261030204733_AddTask3Features'),
    (N'20261031160000_AddInquiryScoringInputs'),
    (N'20261031210000_AddTicketActivityAndDeliveryStatusHistory'),
    (N'20261101151838_SyncSchemaAndAccountLifecycle'),
    (N'20261101152359_ReviewSelfClaimedStaffAccounts'),
    (N'20261102175638_RemoveAccountApprovalGate')
) AS m (MigrationId)
LEFT JOIN [__EFMigrationsHistory] AS h ON h.MigrationId = m.MigrationId
ORDER BY m.MigrationId;

-- Who can sign in, and who is waiting on a decision? Before 22 the second half of
-- that question is ApprovalStatus's; after 22 nobody is waiting, and the only gate
-- left is IsActive, which an administrator sets on /Admin/Users.
IF COL_LENGTH(N'[Users]', N'ApprovalStatus') IS NULL
    PRINT N'ApprovalStatus does not exist - either 20 has not run yet, or 22 has. Both are correct for their moment.';
ELSE
    SELECT Role, ApprovalStatus, COUNT(*) AS Accounts
    FROM [Users]
    GROUP BY Role, ApprovalStatus
    ORDER BY Role, ApprovalStatus;

SELECT Role, IsActive, COUNT(*) AS Accounts
FROM [Users]
GROUP BY Role, IsActive
ORDER BY Role, IsActive;
