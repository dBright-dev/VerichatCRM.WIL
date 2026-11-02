-- ============================================================================
-- Migration 16 of 21: 20261027100000_AddUserDeviceToken
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
        WHERE [MigrationId] = N'20261027100000_AddUserDeviceToken'
    )
    BEGIN
        ALTER TABLE [Users] ADD [DeviceToken] nvarchar(512) NULL;
    END;
    
    IF NOT EXISTS (
        SELECT * FROM [__EFMigrationsHistory]
        WHERE [MigrationId] = N'20261027100000_AddUserDeviceToken'
    )
    BEGIN
        INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
        VALUES (N'20261027100000_AddUserDeviceToken', N'10.0.12');
    END;
    
    COMMIT;

    PRINT N'Step 16/21 - AddUserDeviceToken applied (or already recorded).';
END TRY
BEGIN CATCH
    IF XACT_STATE() <> 0 ROLLBACK TRANSACTION;
    DECLARE @err16 nvarchar(4000) = ERROR_MESSAGE();
    PRINT N'Step 16/21 - AddUserDeviceToken FAILED - rolled back, nothing recorded.';
    PRINT N'      ' + @err16;
    DECLARE @raise16 nvarchar(4000) = REPLACE(@err16, N'%', N'%%');
    RAISERROR(@raise16, 16, 1);
END CATCH;
