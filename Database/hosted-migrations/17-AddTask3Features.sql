-- ============================================================================
-- Migration 17 of 22: 20261030204733_AddTask3Features
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
        WHERE [MigrationId] = N'20261030204733_AddTask3Features'
    )
    BEGIN
        ALTER TABLE [Inquiries] ADD [FollowUpStatus] nvarchar(20) NOT NULL DEFAULT N'';
    END;
    
    IF NOT EXISTS (
        SELECT * FROM [__EFMigrationsHistory]
        WHERE [MigrationId] = N'20261030204733_AddTask3Features'
    )
    BEGIN
        ALTER TABLE [Inquiries] ADD [LastContactDate] datetime2 NULL;
    END;
    
    IF NOT EXISTS (
        SELECT * FROM [__EFMigrationsHistory]
        WHERE [MigrationId] = N'20261030204733_AddTask3Features'
    )
    BEGIN
        ALTER TABLE [Inquiries] ADD [NextFollowUpDate] datetime2 NULL;
    END;
    
    IF NOT EXISTS (
        SELECT * FROM [__EFMigrationsHistory]
        WHERE [MigrationId] = N'20261030204733_AddTask3Features'
    )
    BEGIN
        DECLARE @var5 nvarchar(max);
        SELECT @var5 = QUOTENAME([d].[name])
        FROM [sys].[default_constraints] [d]
        INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
        WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Clients]') AND [c].[name] = N'Sector');
        IF @var5 IS NOT NULL EXEC(N'ALTER TABLE [Clients] DROP CONSTRAINT ' + @var5 + ';');
        ALTER TABLE [Clients] ALTER COLUMN [Sector] nvarchar(100) NOT NULL;
    END;
    
    IF NOT EXISTS (
        SELECT * FROM [__EFMigrationsHistory]
        WHERE [MigrationId] = N'20261030204733_AddTask3Features'
    )
    BEGIN
        DECLARE @var6 nvarchar(max);
        SELECT @var6 = QUOTENAME([d].[name])
        FROM [sys].[default_constraints] [d]
        INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
        WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Clients]') AND [c].[name] = N'ContactEmail');
        IF @var6 IS NOT NULL EXEC(N'ALTER TABLE [Clients] DROP CONSTRAINT ' + @var6 + ';');
        ALTER TABLE [Clients] ALTER COLUMN [ContactEmail] nvarchar(256) NOT NULL;
    END;
    
    IF NOT EXISTS (
        SELECT * FROM [__EFMigrationsHistory]
        WHERE [MigrationId] = N'20261030204733_AddTask3Features'
    )
    BEGIN
        DECLARE @var7 nvarchar(max);
        SELECT @var7 = QUOTENAME([d].[name])
        FROM [sys].[default_constraints] [d]
        INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
        WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Clients]') AND [c].[name] = N'ClientName');
        IF @var7 IS NOT NULL EXEC(N'ALTER TABLE [Clients] DROP CONSTRAINT ' + @var7 + ';');
        ALTER TABLE [Clients] ALTER COLUMN [ClientName] nvarchar(200) NOT NULL;
    END;
    
    IF NOT EXISTS (
        SELECT * FROM [__EFMigrationsHistory]
        WHERE [MigrationId] = N'20261030204733_AddTask3Features'
    )
    BEGIN
        ALTER TABLE [Clients] ADD [Latitude] float NOT NULL DEFAULT 0.0E0;
    END;
    
    IF NOT EXISTS (
        SELECT * FROM [__EFMigrationsHistory]
        WHERE [MigrationId] = N'20261030204733_AddTask3Features'
    )
    BEGIN
        ALTER TABLE [Clients] ADD [Longitude] float NOT NULL DEFAULT 0.0E0;
    END;
    
    IF NOT EXISTS (
        SELECT * FROM [__EFMigrationsHistory]
        WHERE [MigrationId] = N'20261030204733_AddTask3Features'
    )
    BEGIN
        ALTER TABLE [Clients] ADD [RequiresCalibration] bit NOT NULL DEFAULT CAST(0 AS bit);
    END;
    
    IF NOT EXISTS (
        SELECT * FROM [__EFMigrationsHistory]
        WHERE [MigrationId] = N'20261030204733_AddTask3Features'
    )
    BEGIN
        ALTER TABLE [Clients] ADD [SectorType] nvarchar(50) NOT NULL DEFAULT N'';
    END;
    
    IF NOT EXISTS (
        SELECT * FROM [__EFMigrationsHistory]
        WHERE [MigrationId] = N'20261030204733_AddTask3Features'
    )
    BEGIN
        CREATE TABLE [CalibrationSchedules] (
            [ScheduleId] int NOT NULL IDENTITY,
            [ClientId] nvarchar(450) NOT NULL,
            [EquipmentId] nvarchar(450) NOT NULL,
            [ScheduledDate] datetime2 NOT NULL,
            [Status] nvarchar(20) NOT NULL,
            [AssignedTechId] nvarchar(450) NOT NULL,
            CONSTRAINT [PK_CalibrationSchedules] PRIMARY KEY ([ScheduleId]),
            CONSTRAINT [FK_CalibrationSchedules_Assets_EquipmentId] FOREIGN KEY ([EquipmentId]) REFERENCES [Assets] ([AssetId]) ON DELETE NO ACTION,
            CONSTRAINT [FK_CalibrationSchedules_Clients_ClientId] FOREIGN KEY ([ClientId]) REFERENCES [Clients] ([ClientId]) ON DELETE CASCADE,
            CONSTRAINT [FK_CalibrationSchedules_Users_AssignedTechId] FOREIGN KEY ([AssignedTechId]) REFERENCES [Users] ([UserId]) ON DELETE NO ACTION
        );
    END;
    
    IF NOT EXISTS (
        SELECT * FROM [__EFMigrationsHistory]
        WHERE [MigrationId] = N'20261030204733_AddTask3Features'
    )
    BEGIN
        CREATE TABLE [ClientAssignments] (
            [ClientAssignmentId] int NOT NULL IDENTITY,
            [ClientId] nvarchar(450) NOT NULL,
            [SalesRepId] nvarchar(450) NOT NULL,
            [AssignedDate] datetime2 NOT NULL,
            CONSTRAINT [PK_ClientAssignments] PRIMARY KEY ([ClientAssignmentId]),
            CONSTRAINT [FK_ClientAssignments_Clients_ClientId] FOREIGN KEY ([ClientId]) REFERENCES [Clients] ([ClientId]) ON DELETE CASCADE,
            CONSTRAINT [FK_ClientAssignments_Users_SalesRepId] FOREIGN KEY ([SalesRepId]) REFERENCES [Users] ([UserId]) ON DELETE NO ACTION
        );
    END;
    
    IF NOT EXISTS (
        SELECT * FROM [__EFMigrationsHistory]
        WHERE [MigrationId] = N'20261030204733_AddTask3Features'
    )
    BEGIN
        CREATE INDEX [IX_Inquiries_FollowUpStatus] ON [Inquiries] ([FollowUpStatus]);
    END;
    
    IF NOT EXISTS (
        SELECT * FROM [__EFMigrationsHistory]
        WHERE [MigrationId] = N'20261030204733_AddTask3Features'
    )
    BEGIN
        CREATE INDEX [IX_Inquiries_NextFollowUpDate] ON [Inquiries] ([NextFollowUpDate]);
    END;
    
    IF NOT EXISTS (
        SELECT * FROM [__EFMigrationsHistory]
        WHERE [MigrationId] = N'20261030204733_AddTask3Features'
    )
    BEGIN
        CREATE INDEX [IX_Clients_SectorType] ON [Clients] ([SectorType]);
    END;
    
    IF NOT EXISTS (
        SELECT * FROM [__EFMigrationsHistory]
        WHERE [MigrationId] = N'20261030204733_AddTask3Features'
    )
    BEGIN
        CREATE INDEX [IX_CalibrationSchedules_AssignedTechId] ON [CalibrationSchedules] ([AssignedTechId]);
    END;
    
    IF NOT EXISTS (
        SELECT * FROM [__EFMigrationsHistory]
        WHERE [MigrationId] = N'20261030204733_AddTask3Features'
    )
    BEGIN
        CREATE INDEX [IX_CalibrationSchedules_ClientId_ScheduledDate] ON [CalibrationSchedules] ([ClientId], [ScheduledDate]);
    END;
    
    IF NOT EXISTS (
        SELECT * FROM [__EFMigrationsHistory]
        WHERE [MigrationId] = N'20261030204733_AddTask3Features'
    )
    BEGIN
        CREATE INDEX [IX_CalibrationSchedules_EquipmentId] ON [CalibrationSchedules] ([EquipmentId]);
    END;
    
    IF NOT EXISTS (
        SELECT * FROM [__EFMigrationsHistory]
        WHERE [MigrationId] = N'20261030204733_AddTask3Features'
    )
    BEGIN
        CREATE INDEX [IX_CalibrationSchedules_Status] ON [CalibrationSchedules] ([Status]);
    END;
    
    IF NOT EXISTS (
        SELECT * FROM [__EFMigrationsHistory]
        WHERE [MigrationId] = N'20261030204733_AddTask3Features'
    )
    BEGIN
        CREATE INDEX [IX_ClientAssignments_ClientId_SalesRepId_AssignedDate] ON [ClientAssignments] ([ClientId], [SalesRepId], [AssignedDate]);
    END;
    
    IF NOT EXISTS (
        SELECT * FROM [__EFMigrationsHistory]
        WHERE [MigrationId] = N'20261030204733_AddTask3Features'
    )
    BEGIN
        CREATE INDEX [IX_ClientAssignments_SalesRepId] ON [ClientAssignments] ([SalesRepId]);
    END;
    
    IF NOT EXISTS (
        SELECT * FROM [__EFMigrationsHistory]
        WHERE [MigrationId] = N'20261030204733_AddTask3Features'
    )
    BEGIN
        INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
        VALUES (N'20261030204733_AddTask3Features', N'10.0.12');
    END;
    
    COMMIT;

    PRINT N'Step 17/22 - AddTask3Features applied (or already recorded).';
END TRY
BEGIN CATCH
    IF XACT_STATE() <> 0 ROLLBACK TRANSACTION;
    DECLARE @err17 nvarchar(4000) = ERROR_MESSAGE();
    PRINT N'Step 17/22 - AddTask3Features FAILED - rolled back, nothing recorded.';
    PRINT N'      ' + @err17;
    DECLARE @raise17 nvarchar(4000) = REPLACE(@err17, N'%', N'%%');
    RAISERROR(@raise17, 16, 1);
END CATCH;
