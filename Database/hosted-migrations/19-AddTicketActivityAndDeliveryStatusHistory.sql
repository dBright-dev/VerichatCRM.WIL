-- ============================================================================
-- Migration 19 of 21: 20261031210000_AddTicketActivityAndDeliveryStatusHistory
-- Route B step for the hosted database on site103828 (api-site.runasp.net).
--
-- One self-contained batch with no GO separators, because a hosting panel web SQL
-- window sends the text as one batch and GO is client-side only. Run 16 then 17 then
-- 18 then 19 then 20 then 21, one file at a time: a file either commits or rolls back,
-- and the failing file names itself. Re-running a file that already succeeded is a no-op,
-- because every statement is still guarded by IF NOT EXISTS on __EFMigrationsHistory.
--
-- QUOTED_IDENTIFIER is asserted here rather than trusted to the client: sqlcmd leaves it
-- OFF without -I, and a filtered-index CREATE then fails with Msg 1934 part-way through.
-- ============================================================================
SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;

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

    PRINT N'Step 19/21 - AddTicketActivityAndDeliveryStatusHistory applied (or already recorded).';
END TRY
BEGIN CATCH
    IF XACT_STATE() <> 0 ROLLBACK TRANSACTION;
    DECLARE @err19 nvarchar(4000) = ERROR_MESSAGE();
    PRINT N'Step 19/21 - AddTicketActivityAndDeliveryStatusHistory FAILED - rolled back, nothing recorded.';
    PRINT N'      ' + @err19;
    DECLARE @raise19 nvarchar(4000) = REPLACE(@err19, N'%', N'%%');
    RAISERROR(@raise19, 16, 1);
END CATCH;
