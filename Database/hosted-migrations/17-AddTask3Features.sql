-- ============================================================================
-- Migration 17 of 21: 20261030204733_AddTask3Features
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

    PRINT N'Step 17/21 - AddTask3Features applied (or already recorded).';
END TRY
BEGIN CATCH
    IF XACT_STATE() <> 0 ROLLBACK TRANSACTION;
    DECLARE @err17 nvarchar(4000) = ERROR_MESSAGE();
    PRINT N'Step 17/21 - AddTask3Features FAILED - rolled back, nothing recorded.';
    PRINT N'      ' + @err17;
    DECLARE @raise17 nvarchar(4000) = REPLACE(@err17, N'%', N'%%');
    RAISERROR(@raise17, 16, 1);
END CATCH;
