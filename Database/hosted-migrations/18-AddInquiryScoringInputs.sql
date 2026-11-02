-- ============================================================================
-- Migration 18 of 21: 20261031160000_AddInquiryScoringInputs
-- Route B step for the hosted database on site103828 (api-site.runasp.net).
--
-- One self-contained batch with no GO separators, because a hosting panel web
-- SQL window submits its text as one batch and GO is a client-side separator.
-- Run 16, then 17, 18, 19, 20, 21, one file at a time. Every statement keeps the
-- IF NOT EXISTS guard on __EFMigrationsHistory, so a file that already succeeded
-- is a no-op and a file that failed can be re-run once its cause is fixed.
--
-- Two failure shapes, and they are NOT interchangeable (measured 2 Nov):
--   * an execution error - duplicate key on one of the five unique indexes,
--     truncation into a narrowed column, a NULL where the column turns NOT NULL -
--     enters the CATCH below, prints the Step n/21 FAILED line, rolls the whole
--     migration back and re-raises the original message.
--   * a DDL compile error - Msg 2714 There is already an object named, Msg 2705
--     Column names in each table must be unique - ABORTS the batch without ever
--     entering CATCH. You get only the server message, no Step line, and the open
--     transaction from the failed attempt can outlive it on a pooled panel
--     connection. The unconditional ROLLBACK guard below is what clears that.
--   Either way the migration is not recorded, so verify-migration-state.sql tells
--   you where the chain actually stopped. See DEPLOYMENT.md section 8.5.
--
-- QUOTED_IDENTIFIER is asserted rather than trusted to the client: sqlcmd leaves it
-- OFF without -I, migration 20 builds its unique filtered indexes through
-- EXEC(N'CREATE UNIQUE INDEX ...'), and that fails with Msg 1934 when it is off.
-- ============================================================================
SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;

BEGIN TRY
    
    BEGIN TRANSACTION;
    IF NOT EXISTS (
        SELECT * FROM [__EFMigrationsHistory]
        WHERE [MigrationId] = N'20261031160000_AddInquiryScoringInputs'
    )
    BEGIN
        DECLARE @var8 nvarchar(max);
        SELECT @var8 = QUOTENAME([d].[name])
        FROM [sys].[default_constraints] [d]
        INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
        WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Inquiries]') AND [c].[name] = N'Status');
        IF @var8 IS NOT NULL EXEC(N'ALTER TABLE [Inquiries] DROP CONSTRAINT ' + @var8 + ';');
        ALTER TABLE [Inquiries] ALTER COLUMN [Status] nvarchar(30) NOT NULL;
    END;
    
    IF NOT EXISTS (
        SELECT * FROM [__EFMigrationsHistory]
        WHERE [MigrationId] = N'20261031160000_AddInquiryScoringInputs'
    )
    BEGIN
        ALTER TABLE [Inquiries] ADD [Urgency] nvarchar(30) NULL;
    END;
    
    IF NOT EXISTS (
        SELECT * FROM [__EFMigrationsHistory]
        WHERE [MigrationId] = N'20261031160000_AddInquiryScoringInputs'
    )
    BEGIN
        ALTER TABLE [Inquiries] ADD [EstimatedValue] decimal(18,2) NULL;
    END;
    
    IF NOT EXISTS (
        SELECT * FROM [__EFMigrationsHistory]
        WHERE [MigrationId] = N'20261031160000_AddInquiryScoringInputs'
    )
    BEGIN
        CREATE INDEX [IX_Inquiries_Status_Score] ON [Inquiries] ([Status], [Score]);
    END;
    
    IF NOT EXISTS (
        SELECT * FROM [__EFMigrationsHistory]
        WHERE [MigrationId] = N'20261031160000_AddInquiryScoringInputs'
    )
    BEGIN
        INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
        VALUES (N'20261031160000_AddInquiryScoringInputs', N'10.0.12');
    END;
    
    COMMIT;

    PRINT N'Step 18/21 - AddInquiryScoringInputs applied (or already recorded).';
END TRY
BEGIN CATCH
    IF XACT_STATE() <> 0 ROLLBACK TRANSACTION;
    DECLARE @err18 nvarchar(4000) = ERROR_MESSAGE();
    PRINT N'Step 18/21 - AddInquiryScoringInputs FAILED - rolled back, nothing recorded.';
    PRINT N'      ' + @err18;
    DECLARE @raise18 nvarchar(4000) = REPLACE(@err18, N'%', N'%%');
    RAISERROR(@raise18, 16, 1);
END CATCH;
