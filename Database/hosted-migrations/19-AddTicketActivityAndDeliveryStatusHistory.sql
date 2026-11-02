-- ============================================================================
-- Migration 19 of 22: 20261031210000_AddTicketActivityAndDeliveryStatusHistory
-- Route B step for the hosted database on site103828 (api-site.runasp.net).
--
-- One self-contained batch with no GO separators, because a hosting panel web
-- SQL window submits its text as one batch and GO is a client-side separator.
-- Run 16, then 17, 18, 19, 20, 21, 22, one file at a time. Every statement keeps the
-- IF NOT EXISTS guard on __EFMigrationsHistory, so a file that already succeeded
-- is a no-op and a file that failed can be re-run once its cause is fixed.
--
-- Two failure shapes, and they are NOT interchangeable (measured 2 Nov):
--   * an execution error - duplicate key on one of the five unique indexes,
--     truncation into a narrowed column, a NULL where the column turns NOT NULL -
--     enters the CATCH below, prints the Step n/22 FAILED line, rolls the whole
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
        WHERE [MigrationId] = N'20261031210000_AddTicketActivityAndDeliveryStatusHistory'
    )
    BEGIN
        CREATE TABLE [TicketActivity] (
            [ActivityId] nvarchar(450) NOT NULL,
            [TicketId] nvarchar(450) NOT NULL,
            [Action] nvarchar(max) NOT NULL,
            [FromValue] nvarchar(max) NULL,
            [ToValue] nvarchar(max) NULL,
            [Notes] nvarchar(max) NULL,
            [PerformedByUserId] nvarchar(max) NOT NULL,
            [PerformedAt] datetime2 NOT NULL,
            CONSTRAINT [PK_TicketActivity] PRIMARY KEY ([ActivityId]),
            CONSTRAINT [FK_TicketActivity_Tickets_TicketId] FOREIGN KEY ([TicketId]) REFERENCES [Tickets] ([TicketId]) ON DELETE CASCADE
        );
    END;
    
    IF NOT EXISTS (
        SELECT * FROM [__EFMigrationsHistory]
        WHERE [MigrationId] = N'20261031210000_AddTicketActivityAndDeliveryStatusHistory'
    )
    BEGIN
        CREATE INDEX [IX_TicketActivity_TicketId] ON [TicketActivity] ([TicketId]);
    END;
    
    IF NOT EXISTS (
        SELECT * FROM [__EFMigrationsHistory]
        WHERE [MigrationId] = N'20261031210000_AddTicketActivityAndDeliveryStatusHistory'
    )
    BEGIN
        CREATE TABLE [DeliveryStatusUpdate] (
            [UpdateId] nvarchar(450) NOT NULL,
            [DeliveryId] nvarchar(max) NOT NULL,
            [Status] nvarchar(max) NOT NULL,
            [Location] nvarchar(max) NULL,
            [Notes] nvarchar(max) NULL,
            [UpdatedByUserId] nvarchar(max) NOT NULL,
            [UpdatedAt] datetime2 NOT NULL,
            [DeliveryLogDeliveryId] nvarchar(450) NULL,
            CONSTRAINT [PK_DeliveryStatusUpdate] PRIMARY KEY ([UpdateId]),
            CONSTRAINT [FK_DeliveryStatusUpdate_DeliveryLogs_DeliveryLogDeliveryId] FOREIGN KEY ([DeliveryLogDeliveryId]) REFERENCES [DeliveryLogs] ([DeliveryId])
        );
    END;
    
    IF NOT EXISTS (
        SELECT * FROM [__EFMigrationsHistory]
        WHERE [MigrationId] = N'20261031210000_AddTicketActivityAndDeliveryStatusHistory'
    )
    BEGIN
        CREATE INDEX [IX_DeliveryStatusUpdate_DeliveryLogDeliveryId] ON [DeliveryStatusUpdate] ([DeliveryLogDeliveryId]);
    END;
    
    IF NOT EXISTS (
        SELECT * FROM [__EFMigrationsHistory]
        WHERE [MigrationId] = N'20261031210000_AddTicketActivityAndDeliveryStatusHistory'
    )
    BEGIN
        INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
        VALUES (N'20261031210000_AddTicketActivityAndDeliveryStatusHistory', N'10.0.12');
    END;
    
    COMMIT;

    PRINT N'Step 19/22 - AddTicketActivityAndDeliveryStatusHistory applied (or already recorded).';
END TRY
BEGIN CATCH
    IF XACT_STATE() <> 0 ROLLBACK TRANSACTION;
    DECLARE @err19 nvarchar(4000) = ERROR_MESSAGE();
    PRINT N'Step 19/22 - AddTicketActivityAndDeliveryStatusHistory FAILED - rolled back, nothing recorded.';
    PRINT N'      ' + @err19;
    DECLARE @raise19 nvarchar(4000) = REPLACE(@err19, N'%', N'%%');
    RAISERROR(@raise19, 16, 1);
END CATCH;
