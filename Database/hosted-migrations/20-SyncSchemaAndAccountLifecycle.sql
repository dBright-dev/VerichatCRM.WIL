-- ============================================================================
-- Migration 20 of 22: 20261101151838_SyncSchemaAndAccountLifecycle
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
        WHERE [MigrationId] = N'20261101151838_SyncSchemaAndAccountLifecycle'
    )
    BEGIN
        DROP INDEX [IX_Tickets_ClientId] ON [Tickets];
    END;
    
    IF NOT EXISTS (
        SELECT * FROM [__EFMigrationsHistory]
        WHERE [MigrationId] = N'20261101151838_SyncSchemaAndAccountLifecycle'
    )
    BEGIN
        ALTER TABLE [Users] ADD [ApprovalStatus] nvarchar(20) NOT NULL DEFAULT N'Approved';
    END;
    
    IF NOT EXISTS (
        SELECT * FROM [__EFMigrationsHistory]
        WHERE [MigrationId] = N'20261101151838_SyncSchemaAndAccountLifecycle'
    )
    BEGIN
        ALTER TABLE [Users] ADD [EmailVerificationExpiry] datetime2 NULL;
    END;
    
    IF NOT EXISTS (
        SELECT * FROM [__EFMigrationsHistory]
        WHERE [MigrationId] = N'20261101151838_SyncSchemaAndAccountLifecycle'
    )
    BEGIN
        ALTER TABLE [Users] ADD [EmailVerificationToken] nvarchar(256) NULL;
    END;
    
    IF NOT EXISTS (
        SELECT * FROM [__EFMigrationsHistory]
        WHERE [MigrationId] = N'20261101151838_SyncSchemaAndAccountLifecycle'
    )
    BEGIN
        ALTER TABLE [Users] ADD [EmailVerified] bit NOT NULL DEFAULT CAST(0 AS bit);
    END;
    
    IF NOT EXISTS (
        SELECT * FROM [__EFMigrationsHistory]
        WHERE [MigrationId] = N'20261101151838_SyncSchemaAndAccountLifecycle'
    )
    BEGIN
        ALTER TABLE [Users] ADD [RejectionReason] nvarchar(1000) NULL;
    END;
    
    IF NOT EXISTS (
        SELECT * FROM [__EFMigrationsHistory]
        WHERE [MigrationId] = N'20261101151838_SyncSchemaAndAccountLifecycle'
    )
    BEGIN
        ALTER TABLE [Users] ADD [ReviewedAt] datetime2 NULL;
    END;
    
    IF NOT EXISTS (
        SELECT * FROM [__EFMigrationsHistory]
        WHERE [MigrationId] = N'20261101151838_SyncSchemaAndAccountLifecycle'
    )
    BEGIN
        ALTER TABLE [Users] ADD [ReviewedByUserId] nvarchar(450) NULL;
    END;
    
    IF NOT EXISTS (
        SELECT * FROM [__EFMigrationsHistory]
        WHERE [MigrationId] = N'20261101151838_SyncSchemaAndAccountLifecycle'
    )
    BEGIN
        DECLARE @var9 nvarchar(max);
        SELECT @var9 = QUOTENAME([d].[name])
        FROM [sys].[default_constraints] [d]
        INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
        WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Tickets]') AND [c].[name] = N'TrackingNumber');
        IF @var9 IS NOT NULL EXEC(N'ALTER TABLE [Tickets] DROP CONSTRAINT ' + @var9 + ';');
        ALTER TABLE [Tickets] ALTER COLUMN [TrackingNumber] nvarchar(30) NOT NULL;
    END;
    
    IF NOT EXISTS (
        SELECT * FROM [__EFMigrationsHistory]
        WHERE [MigrationId] = N'20261101151838_SyncSchemaAndAccountLifecycle'
    )
    BEGIN
        DECLARE @var10 nvarchar(max);
        SELECT @var10 = QUOTENAME([d].[name])
        FROM [sys].[default_constraints] [d]
        INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
        WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Tickets]') AND [c].[name] = N'Title');
        IF @var10 IS NOT NULL EXEC(N'ALTER TABLE [Tickets] DROP CONSTRAINT ' + @var10 + ';');
        ALTER TABLE [Tickets] ALTER COLUMN [Title] nvarchar(200) NOT NULL;
    END;
    
    IF NOT EXISTS (
        SELECT * FROM [__EFMigrationsHistory]
        WHERE [MigrationId] = N'20261101151838_SyncSchemaAndAccountLifecycle'
    )
    BEGIN
        DECLARE @var11 nvarchar(max);
        SELECT @var11 = QUOTENAME([d].[name])
        FROM [sys].[default_constraints] [d]
        INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
        WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Tickets]') AND [c].[name] = N'Status');
        IF @var11 IS NOT NULL EXEC(N'ALTER TABLE [Tickets] DROP CONSTRAINT ' + @var11 + ';');
        ALTER TABLE [Tickets] ALTER COLUMN [Status] nvarchar(30) NOT NULL;
    END;
    
    IF NOT EXISTS (
        SELECT * FROM [__EFMigrationsHistory]
        WHERE [MigrationId] = N'20261101151838_SyncSchemaAndAccountLifecycle'
    )
    BEGIN
        DECLARE @var12 nvarchar(max);
        SELECT @var12 = QUOTENAME([d].[name])
        FROM [sys].[default_constraints] [d]
        INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
        WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Tickets]') AND [c].[name] = N'ResolutionNotes');
        IF @var12 IS NOT NULL EXEC(N'ALTER TABLE [Tickets] DROP CONSTRAINT ' + @var12 + ';');
        ALTER TABLE [Tickets] ALTER COLUMN [ResolutionNotes] nvarchar(4000) NULL;
    END;
    
    IF NOT EXISTS (
        SELECT * FROM [__EFMigrationsHistory]
        WHERE [MigrationId] = N'20261101151838_SyncSchemaAndAccountLifecycle'
    )
    BEGIN
        DECLARE @var13 nvarchar(max);
        SELECT @var13 = QUOTENAME([d].[name])
        FROM [sys].[default_constraints] [d]
        INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
        WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Tickets]') AND [c].[name] = N'Priority');
        IF @var13 IS NOT NULL EXEC(N'ALTER TABLE [Tickets] DROP CONSTRAINT ' + @var13 + ';');
        ALTER TABLE [Tickets] ALTER COLUMN [Priority] nvarchar(20) NOT NULL;
    END;
    
    IF NOT EXISTS (
        SELECT * FROM [__EFMigrationsHistory]
        WHERE [MigrationId] = N'20261101151838_SyncSchemaAndAccountLifecycle'
    )
    BEGIN
        DECLARE @var14 nvarchar(max);
        SELECT @var14 = QUOTENAME([d].[name])
        FROM [sys].[default_constraints] [d]
        INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
        WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Tickets]') AND [c].[name] = N'Description');
        IF @var14 IS NOT NULL EXEC(N'ALTER TABLE [Tickets] DROP CONSTRAINT ' + @var14 + ';');
        ALTER TABLE [Tickets] ALTER COLUMN [Description] nvarchar(4000) NOT NULL;
    END;
    
    IF NOT EXISTS (
        SELECT * FROM [__EFMigrationsHistory]
        WHERE [MigrationId] = N'20261101151838_SyncSchemaAndAccountLifecycle'
    )
    BEGIN
        DECLARE @var15 nvarchar(max);
        SELECT @var15 = QUOTENAME([d].[name])
        FROM [sys].[default_constraints] [d]
        INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
        WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Tickets]') AND [c].[name] = N'Category');
        IF @var15 IS NOT NULL EXEC(N'ALTER TABLE [Tickets] DROP CONSTRAINT ' + @var15 + ';');
        ALTER TABLE [Tickets] ALTER COLUMN [Category] nvarchar(50) NOT NULL;
    END;
    
    IF NOT EXISTS (
        SELECT * FROM [__EFMigrationsHistory]
        WHERE [MigrationId] = N'20261101151838_SyncSchemaAndAccountLifecycle'
    )
    BEGIN
        DECLARE @var16 nvarchar(max);
        SELECT @var16 = QUOTENAME([d].[name])
        FROM [sys].[default_constraints] [d]
        INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
        WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Tickets]') AND [c].[name] = N'AssignedToUserId');
        IF @var16 IS NOT NULL EXEC(N'ALTER TABLE [Tickets] DROP CONSTRAINT ' + @var16 + ';');
        ALTER TABLE [Tickets] ALTER COLUMN [AssignedToUserId] nvarchar(450) NULL;
    END;
    
    IF NOT EXISTS (
        SELECT * FROM [__EFMigrationsHistory]
        WHERE [MigrationId] = N'20261101151838_SyncSchemaAndAccountLifecycle'
    )
    BEGIN
        DECLARE @var17 nvarchar(max);
        SELECT @var17 = QUOTENAME([d].[name])
        FROM [sys].[default_constraints] [d]
        INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
        WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Quotations]') AND [c].[name] = N'VatRate');
        IF @var17 IS NOT NULL EXEC(N'ALTER TABLE [Quotations] DROP CONSTRAINT ' + @var17 + ';');
        ALTER TABLE [Quotations] ALTER COLUMN [VatRate] decimal(5,4) NOT NULL;
    END;
    
    IF NOT EXISTS (
        SELECT * FROM [__EFMigrationsHistory]
        WHERE [MigrationId] = N'20261101151838_SyncSchemaAndAccountLifecycle'
    )
    BEGIN
        DECLARE @var18 nvarchar(max);
        SELECT @var18 = QUOTENAME([d].[name])
        FROM [sys].[default_constraints] [d]
        INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
        WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Quotations]') AND [c].[name] = N'Terms');
        IF @var18 IS NOT NULL EXEC(N'ALTER TABLE [Quotations] DROP CONSTRAINT ' + @var18 + ';');
        ALTER TABLE [Quotations] ALTER COLUMN [Terms] nvarchar(4000) NOT NULL;
    END;
    
    IF NOT EXISTS (
        SELECT * FROM [__EFMigrationsHistory]
        WHERE [MigrationId] = N'20261101151838_SyncSchemaAndAccountLifecycle'
    )
    BEGIN
        DECLARE @var19 nvarchar(max);
        SELECT @var19 = QUOTENAME([d].[name])
        FROM [sys].[default_constraints] [d]
        INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
        WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Quotations]') AND [c].[name] = N'Status');
        IF @var19 IS NOT NULL EXEC(N'ALTER TABLE [Quotations] DROP CONSTRAINT ' + @var19 + ';');
        ALTER TABLE [Quotations] ALTER COLUMN [Status] nvarchar(30) NOT NULL;
    END;
    
    IF NOT EXISTS (
        SELECT * FROM [__EFMigrationsHistory]
        WHERE [MigrationId] = N'20261101151838_SyncSchemaAndAccountLifecycle'
    )
    BEGIN
        DECLARE @var20 nvarchar(max);
        SELECT @var20 = QUOTENAME([d].[name])
        FROM [sys].[default_constraints] [d]
        INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
        WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Quotations]') AND [c].[name] = N'QuoteReference');
        IF @var20 IS NOT NULL EXEC(N'ALTER TABLE [Quotations] DROP CONSTRAINT ' + @var20 + ';');
        ALTER TABLE [Quotations] ALTER COLUMN [QuoteReference] nvarchar(50) NOT NULL;
    END;
    
    IF NOT EXISTS (
        SELECT * FROM [__EFMigrationsHistory]
        WHERE [MigrationId] = N'20261101151838_SyncSchemaAndAccountLifecycle'
    )
    BEGIN
        DECLARE @var21 nvarchar(max);
        SELECT @var21 = QUOTENAME([d].[name])
        FROM [sys].[default_constraints] [d]
        INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
        WHERE ([d].[parent_object_id] = OBJECT_ID(N'[QuotationLineItem]') AND [c].[name] = N'SKU');
        IF @var21 IS NOT NULL EXEC(N'ALTER TABLE [QuotationLineItem] DROP CONSTRAINT ' + @var21 + ';');
        ALTER TABLE [QuotationLineItem] ALTER COLUMN [SKU] nvarchar(50) NOT NULL;
    END;
    
    IF NOT EXISTS (
        SELECT * FROM [__EFMigrationsHistory]
        WHERE [MigrationId] = N'20261101151838_SyncSchemaAndAccountLifecycle'
    )
    BEGIN
        DECLARE @var22 nvarchar(max);
        SELECT @var22 = QUOTENAME([d].[name])
        FROM [sys].[default_constraints] [d]
        INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
        WHERE ([d].[parent_object_id] = OBJECT_ID(N'[QuotationLineItem]') AND [c].[name] = N'ProductName');
        IF @var22 IS NOT NULL EXEC(N'ALTER TABLE [QuotationLineItem] DROP CONSTRAINT ' + @var22 + ';');
        ALTER TABLE [QuotationLineItem] ALTER COLUMN [ProductName] nvarchar(200) NOT NULL;
    END;
    
    IF NOT EXISTS (
        SELECT * FROM [__EFMigrationsHistory]
        WHERE [MigrationId] = N'20261101151838_SyncSchemaAndAccountLifecycle'
    )
    BEGIN
        DECLARE @var23 nvarchar(max);
        SELECT @var23 = QUOTENAME([d].[name])
        FROM [sys].[default_constraints] [d]
        INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
        WHERE ([d].[parent_object_id] = OBJECT_ID(N'[DeliveryLogs]') AND [c].[name] = N'WeightKg');
        IF @var23 IS NOT NULL EXEC(N'ALTER TABLE [DeliveryLogs] DROP CONSTRAINT ' + @var23 + ';');
        ALTER TABLE [DeliveryLogs] ALTER COLUMN [WeightKg] decimal(10,2) NOT NULL;
    END;
    
    IF NOT EXISTS (
        SELECT * FROM [__EFMigrationsHistory]
        WHERE [MigrationId] = N'20261101151838_SyncSchemaAndAccountLifecycle'
    )
    BEGIN
        DECLARE @var24 nvarchar(max);
        SELECT @var24 = QUOTENAME([d].[name])
        FROM [sys].[default_constraints] [d]
        INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
        WHERE ([d].[parent_object_id] = OBJECT_ID(N'[DeliveryLogs]') AND [c].[name] = N'TrackingNumber');
        IF @var24 IS NOT NULL EXEC(N'ALTER TABLE [DeliveryLogs] DROP CONSTRAINT ' + @var24 + ';');
        ALTER TABLE [DeliveryLogs] ALTER COLUMN [TrackingNumber] nvarchar(30) NOT NULL;
    END;
    
    IF NOT EXISTS (
        SELECT * FROM [__EFMigrationsHistory]
        WHERE [MigrationId] = N'20261101151838_SyncSchemaAndAccountLifecycle'
    )
    BEGIN
        DECLARE @var25 nvarchar(max);
        SELECT @var25 = QUOTENAME([d].[name])
        FROM [sys].[default_constraints] [d]
        INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
        WHERE ([d].[parent_object_id] = OBJECT_ID(N'[DeliveryLogs]') AND [c].[name] = N'Status');
        IF @var25 IS NOT NULL EXEC(N'ALTER TABLE [DeliveryLogs] DROP CONSTRAINT ' + @var25 + ';');
        ALTER TABLE [DeliveryLogs] ALTER COLUMN [Status] nvarchar(30) NOT NULL;
    END;
    
    IF NOT EXISTS (
        SELECT * FROM [__EFMigrationsHistory]
        WHERE [MigrationId] = N'20261101151838_SyncSchemaAndAccountLifecycle'
    )
    BEGIN
        DECLARE @var26 nvarchar(max);
        SELECT @var26 = QUOTENAME([d].[name])
        FROM [sys].[default_constraints] [d]
        INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
        WHERE ([d].[parent_object_id] = OBJECT_ID(N'[DeliveryLogs]') AND [c].[name] = N'Origin');
        IF @var26 IS NOT NULL EXEC(N'ALTER TABLE [DeliveryLogs] DROP CONSTRAINT ' + @var26 + ';');
        ALTER TABLE [DeliveryLogs] ALTER COLUMN [Origin] nvarchar(200) NOT NULL;
    END;
    
    IF NOT EXISTS (
        SELECT * FROM [__EFMigrationsHistory]
        WHERE [MigrationId] = N'20261101151838_SyncSchemaAndAccountLifecycle'
    )
    BEGIN
        DECLARE @var27 nvarchar(max);
        SELECT @var27 = QUOTENAME([d].[name])
        FROM [sys].[default_constraints] [d]
        INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
        WHERE ([d].[parent_object_id] = OBJECT_ID(N'[DeliveryLogs]') AND [c].[name] = N'Destination');
        IF @var27 IS NOT NULL EXEC(N'ALTER TABLE [DeliveryLogs] DROP CONSTRAINT ' + @var27 + ';');
        ALTER TABLE [DeliveryLogs] ALTER COLUMN [Destination] nvarchar(200) NOT NULL;
    END;
    
    IF NOT EXISTS (
        SELECT * FROM [__EFMigrationsHistory]
        WHERE [MigrationId] = N'20261101151838_SyncSchemaAndAccountLifecycle'
    )
    BEGIN
        DECLARE @var28 nvarchar(max);
        SELECT @var28 = QUOTENAME([d].[name])
        FROM [sys].[default_constraints] [d]
        INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
        WHERE ([d].[parent_object_id] = OBJECT_ID(N'[DeliveryLogs]') AND [c].[name] = N'ClientId');
        IF @var28 IS NOT NULL EXEC(N'ALTER TABLE [DeliveryLogs] DROP CONSTRAINT ' + @var28 + ';');
        ALTER TABLE [DeliveryLogs] ALTER COLUMN [ClientId] nvarchar(450) NOT NULL;
    END;
    
    IF NOT EXISTS (
        SELECT * FROM [__EFMigrationsHistory]
        WHERE [MigrationId] = N'20261101151838_SyncSchemaAndAccountLifecycle'
    )
    BEGIN
        DECLARE @var29 nvarchar(max);
        SELECT @var29 = QUOTENAME([d].[name])
        FROM [sys].[default_constraints] [d]
        INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
        WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Deals]') AND [c].[name] = N'Stage');
        IF @var29 IS NOT NULL EXEC(N'ALTER TABLE [Deals] DROP CONSTRAINT ' + @var29 + ';');
        ALTER TABLE [Deals] ALTER COLUMN [Stage] nvarchar(50) NOT NULL;
    END;
    
    IF NOT EXISTS (
        SELECT * FROM [__EFMigrationsHistory]
        WHERE [MigrationId] = N'20261101151838_SyncSchemaAndAccountLifecycle'
    )
    BEGIN
        DECLARE @var30 nvarchar(max);
        SELECT @var30 = QUOTENAME([d].[name])
        FROM [sys].[default_constraints] [d]
        INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
        WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Deals]') AND [c].[name] = N'Notes');
        IF @var30 IS NOT NULL EXEC(N'ALTER TABLE [Deals] DROP CONSTRAINT ' + @var30 + ';');
        ALTER TABLE [Deals] ALTER COLUMN [Notes] nvarchar(4000) NULL;
    END;
    
    IF NOT EXISTS (
        SELECT * FROM [__EFMigrationsHistory]
        WHERE [MigrationId] = N'20261101151838_SyncSchemaAndAccountLifecycle'
    )
    BEGIN
        DECLARE @var31 nvarchar(max);
        SELECT @var31 = QUOTENAME([d].[name])
        FROM [sys].[default_constraints] [d]
        INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
        WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Clients]') AND [c].[name] = N'ContractStatus');
        IF @var31 IS NOT NULL EXEC(N'ALTER TABLE [Clients] DROP CONSTRAINT ' + @var31 + ';');
        ALTER TABLE [Clients] ALTER COLUMN [ContractStatus] nvarchar(50) NOT NULL;
    END;
    
    IF NOT EXISTS (
        SELECT * FROM [__EFMigrationsHistory]
        WHERE [MigrationId] = N'20261101151838_SyncSchemaAndAccountLifecycle'
    )
    BEGIN
        DECLARE @var32 nvarchar(max);
        SELECT @var32 = QUOTENAME([d].[name])
        FROM [sys].[default_constraints] [d]
        INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
        WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Clients]') AND [c].[name] = N'ContactPhone');
        IF @var32 IS NOT NULL EXEC(N'ALTER TABLE [Clients] DROP CONSTRAINT ' + @var32 + ';');
        ALTER TABLE [Clients] ALTER COLUMN [ContactPhone] nvarchar(30) NOT NULL;
    END;
    
    IF NOT EXISTS (
        SELECT * FROM [__EFMigrationsHistory]
        WHERE [MigrationId] = N'20261101151838_SyncSchemaAndAccountLifecycle'
    )
    BEGIN
        DECLARE @var33 nvarchar(max);
        SELECT @var33 = QUOTENAME([d].[name])
        FROM [sys].[default_constraints] [d]
        INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
        WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Clients]') AND [c].[name] = N'ContactPerson');
        IF @var33 IS NOT NULL EXEC(N'ALTER TABLE [Clients] DROP CONSTRAINT ' + @var33 + ';');
        ALTER TABLE [Clients] ALTER COLUMN [ContactPerson] nvarchar(150) NOT NULL;
    END;
    
    IF NOT EXISTS (
        SELECT * FROM [__EFMigrationsHistory]
        WHERE [MigrationId] = N'20261101151838_SyncSchemaAndAccountLifecycle'
    )
    BEGIN
        DECLARE @var34 nvarchar(max);
        SELECT @var34 = QUOTENAME([d].[name])
        FROM [sys].[default_constraints] [d]
        INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
        WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Clients]') AND [c].[name] = N'Address');
        IF @var34 IS NOT NULL EXEC(N'ALTER TABLE [Clients] DROP CONSTRAINT ' + @var34 + ';');
        ALTER TABLE [Clients] ALTER COLUMN [Address] nvarchar(500) NOT NULL;
    END;
    
    IF NOT EXISTS (
        SELECT * FROM [__EFMigrationsHistory]
        WHERE [MigrationId] = N'20261101151838_SyncSchemaAndAccountLifecycle'
    )
    BEGIN
        DECLARE @var35 nvarchar(max);
        SELECT @var35 = QUOTENAME([d].[name])
        FROM [sys].[default_constraints] [d]
        INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
        WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Assets]') AND [c].[name] = N'Status');
        IF @var35 IS NOT NULL EXEC(N'ALTER TABLE [Assets] DROP CONSTRAINT ' + @var35 + ';');
        ALTER TABLE [Assets] ALTER COLUMN [Status] nvarchar(30) NOT NULL;
    END;
    
    IF NOT EXISTS (
        SELECT * FROM [__EFMigrationsHistory]
        WHERE [MigrationId] = N'20261101151838_SyncSchemaAndAccountLifecycle'
    )
    BEGIN
        DECLARE @var36 nvarchar(max);
        SELECT @var36 = QUOTENAME([d].[name])
        FROM [sys].[default_constraints] [d]
        INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
        WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Assets]') AND [c].[name] = N'SerialNumber');
        IF @var36 IS NOT NULL EXEC(N'ALTER TABLE [Assets] DROP CONSTRAINT ' + @var36 + ';');
        ALTER TABLE [Assets] ALTER COLUMN [SerialNumber] nvarchar(100) NOT NULL;
    END;
    
    IF NOT EXISTS (
        SELECT * FROM [__EFMigrationsHistory]
        WHERE [MigrationId] = N'20261101151838_SyncSchemaAndAccountLifecycle'
    )
    BEGIN
        DECLARE @var37 nvarchar(max);
        SELECT @var37 = QUOTENAME([d].[name])
        FROM [sys].[default_constraints] [d]
        INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
        WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Assets]') AND [c].[name] = N'Model');
        IF @var37 IS NOT NULL EXEC(N'ALTER TABLE [Assets] DROP CONSTRAINT ' + @var37 + ';');
        ALTER TABLE [Assets] ALTER COLUMN [Model] nvarchar(200) NOT NULL;
    END;
    
    IF NOT EXISTS (
        SELECT * FROM [__EFMigrationsHistory]
        WHERE [MigrationId] = N'20261101151838_SyncSchemaAndAccountLifecycle'
    )
    BEGIN
        DECLARE @var38 nvarchar(max);
        SELECT @var38 = QUOTENAME([d].[name])
        FROM [sys].[default_constraints] [d]
        INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
        WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Assets]') AND [c].[name] = N'Manufacturer');
        IF @var38 IS NOT NULL EXEC(N'ALTER TABLE [Assets] DROP CONSTRAINT ' + @var38 + ';');
        ALTER TABLE [Assets] ALTER COLUMN [Manufacturer] nvarchar(200) NOT NULL;
    END;
    
    IF NOT EXISTS (
        SELECT * FROM [__EFMigrationsHistory]
        WHERE [MigrationId] = N'20261101151838_SyncSchemaAndAccountLifecycle'
    )
    BEGIN
        CREATE TABLE [LoginHelpRequests] (
            [Id] nvarchar(450) NOT NULL,
            [Name] nvarchar(150) NOT NULL,
            [Email] nvarchar(200) NOT NULL,
            [ExpectedRole] nvarchar(50) NULL,
            [Message] nvarchar(2000) NOT NULL,
            [Channel] nvarchar(40) NOT NULL,
            [CreatedAt] datetime2 NOT NULL,
            [Status] nvarchar(20) NOT NULL,
            [ResolvedByUserId] nvarchar(450) NULL,
            [ResolvedAt] datetime2 NULL,
            CONSTRAINT [PK_LoginHelpRequests] PRIMARY KEY ([Id])
        );
    END;
    
    IF NOT EXISTS (
        SELECT * FROM [__EFMigrationsHistory]
        WHERE [MigrationId] = N'20261101151838_SyncSchemaAndAccountLifecycle'
    )
    BEGIN
        CREATE INDEX [IX_Users_ApprovalStatus_CreatedAt] ON [Users] ([ApprovalStatus], [CreatedAt]);
    END;
    
    IF NOT EXISTS (
        SELECT * FROM [__EFMigrationsHistory]
        WHERE [MigrationId] = N'20261101151838_SyncSchemaAndAccountLifecycle'
    )
    BEGIN
        CREATE UNIQUE INDEX [IX_Users_Email] ON [Users] ([Email]);
    END;
    
    IF NOT EXISTS (
        SELECT * FROM [__EFMigrationsHistory]
        WHERE [MigrationId] = N'20261101151838_SyncSchemaAndAccountLifecycle'
    )
    BEGIN
        CREATE INDEX [IX_Tickets_AssignedToUserId_Status] ON [Tickets] ([AssignedToUserId], [Status]);
    END;
    
    IF NOT EXISTS (
        SELECT * FROM [__EFMigrationsHistory]
        WHERE [MigrationId] = N'20261101151838_SyncSchemaAndAccountLifecycle'
    )
    BEGIN
        CREATE INDEX [IX_Tickets_ClientId_Status] ON [Tickets] ([ClientId], [Status]);
    END;
    
    IF NOT EXISTS (
        SELECT * FROM [__EFMigrationsHistory]
        WHERE [MigrationId] = N'20261101151838_SyncSchemaAndAccountLifecycle'
    )
    BEGIN
        CREATE INDEX [IX_Tickets_SlaDeadline] ON [Tickets] ([SlaDeadline]);
    END;
    
    IF NOT EXISTS (
        SELECT * FROM [__EFMigrationsHistory]
        WHERE [MigrationId] = N'20261101151838_SyncSchemaAndAccountLifecycle'
    )
    BEGIN
        EXEC(N'CREATE UNIQUE INDEX [IX_Tickets_TrackingNumber] ON [Tickets] ([TrackingNumber]) WHERE [TrackingNumber] <> N''''');
    END;
    
    IF NOT EXISTS (
        SELECT * FROM [__EFMigrationsHistory]
        WHERE [MigrationId] = N'20261101151838_SyncSchemaAndAccountLifecycle'
    )
    BEGIN
        EXEC(N'CREATE UNIQUE INDEX [IX_Quotations_QuoteReference] ON [Quotations] ([QuoteReference]) WHERE [QuoteReference] <> N''''');
    END;
    
    IF NOT EXISTS (
        SELECT * FROM [__EFMigrationsHistory]
        WHERE [MigrationId] = N'20261101151838_SyncSchemaAndAccountLifecycle'
    )
    BEGIN
        CREATE INDEX [IX_DeliveryLogs_ClientId_Status] ON [DeliveryLogs] ([ClientId], [Status]);
    END;
    
    IF NOT EXISTS (
        SELECT * FROM [__EFMigrationsHistory]
        WHERE [MigrationId] = N'20261101151838_SyncSchemaAndAccountLifecycle'
    )
    BEGIN
        EXEC(N'CREATE UNIQUE INDEX [IX_DeliveryLogs_TrackingNumber] ON [DeliveryLogs] ([TrackingNumber]) WHERE [TrackingNumber] <> N''''');
    END;
    
    IF NOT EXISTS (
        SELECT * FROM [__EFMigrationsHistory]
        WHERE [MigrationId] = N'20261101151838_SyncSchemaAndAccountLifecycle'
    )
    BEGIN
        CREATE INDEX [IX_Deals_Stage] ON [Deals] ([Stage]);
    END;
    
    IF NOT EXISTS (
        SELECT * FROM [__EFMigrationsHistory]
        WHERE [MigrationId] = N'20261101151838_SyncSchemaAndAccountLifecycle'
    )
    BEGIN
        CREATE INDEX [IX_Clients_ContractStatus] ON [Clients] ([ContractStatus]);
    END;
    
    IF NOT EXISTS (
        SELECT * FROM [__EFMigrationsHistory]
        WHERE [MigrationId] = N'20261101151838_SyncSchemaAndAccountLifecycle'
    )
    BEGIN
        CREATE INDEX [IX_Clients_Sector] ON [Clients] ([Sector]);
    END;
    
    IF NOT EXISTS (
        SELECT * FROM [__EFMigrationsHistory]
        WHERE [MigrationId] = N'20261101151838_SyncSchemaAndAccountLifecycle'
    )
    BEGIN
        EXEC(N'CREATE UNIQUE INDEX [IX_Assets_SerialNumber] ON [Assets] ([SerialNumber]) WHERE [SerialNumber] <> N''''');
    END;
    
    IF NOT EXISTS (
        SELECT * FROM [__EFMigrationsHistory]
        WHERE [MigrationId] = N'20261101151838_SyncSchemaAndAccountLifecycle'
    )
    BEGIN
        CREATE INDEX [IX_Assets_WarrantyExpiry] ON [Assets] ([WarrantyExpiry]);
    END;
    
    IF NOT EXISTS (
        SELECT * FROM [__EFMigrationsHistory]
        WHERE [MigrationId] = N'20261101151838_SyncSchemaAndAccountLifecycle'
    )
    BEGIN
        CREATE INDEX [IX_LoginHelpRequests_Status_CreatedAt] ON [LoginHelpRequests] ([Status], [CreatedAt]);
    END;
    
    IF NOT EXISTS (
        SELECT * FROM [__EFMigrationsHistory]
        WHERE [MigrationId] = N'20261101151838_SyncSchemaAndAccountLifecycle'
    )
    BEGIN
        INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
        VALUES (N'20261101151838_SyncSchemaAndAccountLifecycle', N'10.0.12');
    END;
    
    COMMIT;

    PRINT N'Step 20/22 - SyncSchemaAndAccountLifecycle applied (or already recorded).';
END TRY
BEGIN CATCH
    IF XACT_STATE() <> 0 ROLLBACK TRANSACTION;
    DECLARE @err20 nvarchar(4000) = ERROR_MESSAGE();
    PRINT N'Step 20/22 - SyncSchemaAndAccountLifecycle FAILED - rolled back, nothing recorded.';
    PRINT N'      ' + @err20;
    DECLARE @raise20 nvarchar(4000) = REPLACE(@err20, N'%', N'%%');
    RAISERROR(@raise20, 16, 1);
END CATCH;
