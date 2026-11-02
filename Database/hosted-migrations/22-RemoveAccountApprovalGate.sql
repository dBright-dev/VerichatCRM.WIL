-- ============================================================================
-- Migration 22 of 22: 20261102175638_RemoveAccountApprovalGate
-- Route B step for the hosted database on site103828 (api-site.runasp.net).
--
-- The employee-approval gate is gone from the application, so the four columns it
-- lived in and the queue index that read them go here too. What replaces the review
-- is a narrower door rather than a weaker one: the sign-up form can only mint
-- Client User and Medical Student, and every staff account is created by an
-- authenticated administrator on /Admin/Users.
--
-- Read the first statement before reading the rest. Deploying the new code is the
-- whole of the removal - nothing consults ApprovalStatus any more - so without this
-- file the hosted database would silently promote every account migration 21 parked
-- in Pending, self-claimed staff roles included. The UPDATE withdraws access from
-- exactly those rows first; they stay in the table, disabled, for an administrator
-- to judge on the users screen. Approved and Rejected rows are untouched, and the
-- Rejected ones were already out through IsActive.
--
-- One self-contained batch with no GO separators, because a hosting panel web SQL
-- window submits its text as one batch and GO is a client-side separator. Run 16,
-- then 17, 18, 19, 20, 21, 22, one file at a time. Every statement keeps the
-- IF NOT EXISTS guard on __EFMigrationsHistory, so a file that already succeeded is
-- a no-op and a file that failed can be re-run once its cause is fixed.
--
-- The column references are inside EXEC(N'...') for that re-run promise. T-SQL
-- resolves column names when the batch is compiled, not when the IF branch is
-- reached, so a plain UPDATE ... WHERE [ApprovalStatus] = ... on a database where
-- this migration already succeeded aborts with Msg 207 Invalid column name before
-- the guard is ever evaluated. Dynamic SQL defers that lookup to the moment the
-- statement runs, which is inside the guard - and each drop is additionally checked
-- against sys.columns and sys.indexes, because this file is also the fix for a
-- database that got partway through it once.
--
-- Two failure shapes, and they are NOT interchangeable (measured 2 Nov, on 21):
--   * an execution error - a permission denial on the UPDATE, a lock timeout, a
--     column still referenced by something this file did not expect - enters the
--     CATCH below, prints the Step n/22 FAILED line, rolls the whole migration back
--     and re-raises the original message.
--   * a DDL compile error - Msg 2714 There is already an object named, Msg 2705
--     Column names in each table must be unique - ABORTS the batch without ever
--     entering CATCH. You get only the server message, no Step line, and the open
--     transaction from the failed attempt can outlive it on a pooled panel
--     connection. The unconditional ROLLBACK guard below is what clears that.
--   Either way the migration is not recorded, so verify-migration-state.sql tells
--   you where the chain actually stopped. See DEPLOYMENT.md section 8.5.
--
-- QUOTED_IDENTIFIER is asserted rather than trusted to the client: sqlcmd leaves it
-- OFF without -I, and the ALTER TABLE ... DROP COLUMN statements below fail with
-- Msg 1934 on a table that carries a filtered index or a computed column.
-- ============================================================================
SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;

BEGIN TRY

    BEGIN TRANSACTION;
    IF NOT EXISTS (
        SELECT * FROM [__EFMigrationsHistory]
        WHERE [MigrationId] = N'20261102175638_RemoveAccountApprovalGate'
    )
    BEGIN
        -- Unapproved applications lose access before the evidence for losing it goes.
        EXEC(N'UPDATE [Users] SET [IsActive] = 0 WHERE [ApprovalStatus] = N''Pending'';');

        IF EXISTS (
            SELECT 1 FROM [sys].[indexes]
            WHERE [name] = N'IX_Users_ApprovalStatus_CreatedAt'
              AND [object_id] = OBJECT_ID(N'[dbo].[Users]')
        )
            EXEC(N'DROP INDEX [IX_Users_ApprovalStatus_CreatedAt] ON [dbo].[Users];');

        -- The four columns go in a loop over sys.columns rather than four written
        -- statements, for two reasons. The first is re-runnability, which the EXEC
        -- wrappers above already gave them. The second is what this file was corrected
        -- for: ApprovalStatus arrived as NOT NULL DEFAULT N'Approved', so it owns a
        -- default constraint, and SQL Server refuses the drop with "ALTER TABLE DROP
        -- COLUMN ApprovalStatus failed because one or more objects access this column"
        -- until that constraint goes first. Measured 2 Nov on a throwaway database
        -- frozen at migration 21 - the whole batch rolled back, nothing recorded. The
        -- EF tooling emits that pre-step automatically in its own scripts; a hand-run
        -- panel file has to say it out loud, and it has to do it per column because the
        -- constraint name is a generated hash, different in every database.
        DECLARE @col22 nvarchar(256);
        DECLARE @colq22 nvarchar(256);
        DECLARE @dc22  nvarchar(512);
        DECLARE gate22 CURSOR LOCAL FAST_FORWARD FOR
            SELECT [name] FROM [sys].[columns]
            WHERE [object_id] = OBJECT_ID(N'[dbo].[Users]')
              AND [name] IN (N'ApprovalStatus', N'RejectionReason', N'ReviewedAt', N'ReviewedByUserId')
            ORDER BY [column_id] DESC;
        OPEN gate22;
        FETCH NEXT FROM gate22 INTO @col22;
        WHILE @@FETCH_STATUS = 0
        BEGIN
            SET @dc22 = NULL;
            SELECT @dc22 = QUOTENAME([d].[name])
            FROM [sys].[default_constraints] AS [d]
            INNER JOIN [sys].[columns] AS [c]
                ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
            WHERE [d].[parent_object_id] = OBJECT_ID(N'[dbo].[Users]') AND [c].[name] = @col22;
            IF @dc22 IS NOT NULL
                EXEC(N'ALTER TABLE [dbo].[Users] DROP CONSTRAINT ' + @dc22 + N';');
            SET @colq22 = QUOTENAME(@col22);
            EXEC(N'ALTER TABLE [dbo].[Users] DROP COLUMN ' + @colq22 + N';');
            FETCH NEXT FROM gate22 INTO @col22;
        END;
        CLOSE gate22;
        DEALLOCATE gate22;
    END;

    IF NOT EXISTS (
        SELECT * FROM [__EFMigrationsHistory]
        WHERE [MigrationId] = N'20261102175638_RemoveAccountApprovalGate'
    )
    BEGIN
        INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
        VALUES (N'20261102175638_RemoveAccountApprovalGate', N'10.0.12');
    END;

    COMMIT;

    PRINT N'Step 22/22 - RemoveAccountApprovalGate applied (or already recorded).';
    PRINT N'      Users is down to its non-approval columns; Pending applications are now disabled rows.';
END TRY
BEGIN CATCH
    IF XACT_STATE() <> 0 ROLLBACK TRANSACTION;
    DECLARE @err22 nvarchar(4000) = ERROR_MESSAGE();
    PRINT N'Step 22/22 - RemoveAccountApprovalGate FAILED - rolled back, nothing recorded.';
    PRINT N'      ' + @err22;
    DECLARE @raise22 nvarchar(4000) = REPLACE(@err22, N'%', N'%%');
    RAISERROR(@raise22, 16, 1);
END CATCH;
