-- ============================================================================
-- Migration 21 of 21: 20261101152359_ReviewSelfClaimedStaffAccounts
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
        WHERE [MigrationId] = N'20261101152359_ReviewSelfClaimedStaffAccounts'
    )
    BEGIN
        UPDATE [Users]
                          SET [ApprovalStatus] = N'Pending'
                          WHERE [Role] IN (N'Sales Representative', N'Procurement Manager',
                                           N'Field Support Engineer', N'Logistics');
    END;
    
    IF NOT EXISTS (
        SELECT * FROM [__EFMigrationsHistory]
        WHERE [MigrationId] = N'20261101152359_ReviewSelfClaimedStaffAccounts'
    )
    BEGIN
        INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
        VALUES (N'20261101152359_ReviewSelfClaimedStaffAccounts', N'10.0.12');
    END;
    
    COMMIT;

    PRINT N'Step 21/21 - ReviewSelfClaimedStaffAccounts applied (or already recorded).';
END TRY
BEGIN CATCH
    IF XACT_STATE() <> 0 ROLLBACK TRANSACTION;
    DECLARE @err21 nvarchar(4000) = ERROR_MESSAGE();
    PRINT N'Step 21/21 - ReviewSelfClaimedStaffAccounts FAILED - rolled back, nothing recorded.';
    PRINT N'      ' + @err21;
    DECLARE @raise21 nvarchar(4000) = REPLACE(@err21, N'%', N'%%');
    RAISERROR(@raise21, 16, 1);
END CATCH;
