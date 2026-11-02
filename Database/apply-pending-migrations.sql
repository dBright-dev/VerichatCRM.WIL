-- Apply every EF Core migration that the target database has not yet recorded.
--
-- Generated with:
--   dotnet ef migrations script --idempotent --project VerichatCRM.Infrastructure `
--     --startup-project VerichatCRM.API --output Database/apply-pending-migrations.sql
-- and then this header was prepended. Regenerate rather than hand-edit: the body is
-- machine output and edits to it will be lost the next time it is produced.
--
-- Every statement is wrapped in
--   IF NOT EXISTS (SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'...')
-- so migrations already recorded are skipped and only the missing ones run. Re-running
-- this file changes nothing, which was verified by executing it twice against the same
-- database: the second run reported no errors and left 21 migrations and 39 tables.
--
-- The SET line below is not decoration. Without QUOTED_IDENTIFIER ON the batch stops at
-- migration 19 of 21 with "Msg 1934 ... CREATE INDEX failed because the following SET
-- options have incorrect settings: 'QUOTED_IDENTIFIER'", and it stops halfway with the
-- earlier transactions already committed - a database that is neither the old schema nor
-- the new one. sqlcmd runs with the option OFF unless given -I, and a hosting panel's SQL
-- window may do the same, so the setting is asserted here instead of being left to the
-- client. See DEPLOYMENT.md section 8.

SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
GO

IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260928155836_InitialCreate'
)
BEGIN
    CREATE TABLE [Orders] (
        [OrderId] nvarchar(450) NOT NULL,
        [ClientId] nvarchar(450) NOT NULL,
        [DealId] nvarchar(max) NULL,
        [TotalAmount] decimal(18,2) NOT NULL,
        [Status] nvarchar(450) NOT NULL,
        [OrderDate] datetime2 NOT NULL,
        [ExpectedDeliveryDate] datetime2 NULL,
        [TrackingNumber] nvarchar(max) NULL,
        CONSTRAINT [PK_Orders] PRIMARY KEY ([OrderId])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260928155836_InitialCreate'
)
BEGIN
    CREATE TABLE [Products] (
        [SKU] nvarchar(450) NOT NULL,
        [ProductName] nvarchar(200) NOT NULL,
        [Category] nvarchar(100) NOT NULL,
        [Price] decimal(18,2) NOT NULL,
        [StockQty] int NOT NULL,
        [LowStockThreshold] int NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        CONSTRAINT [PK_Products] PRIMARY KEY ([SKU])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260928155836_InitialCreate'
)
BEGIN
    CREATE TABLE [PurchaseOrders] (
        [PurchaseOrderId] nvarchar(450) NOT NULL,
        [SupplierName] nvarchar(max) NOT NULL,
        [TotalCost] decimal(18,2) NOT NULL,
        [Status] nvarchar(max) NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [ExpectedArrivalDate] datetime2 NULL,
        CONSTRAINT [PK_PurchaseOrders] PRIMARY KEY ([PurchaseOrderId])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260928155836_InitialCreate'
)
BEGIN
    CREATE TABLE [WarehouseAlerts] (
        [AlertId] nvarchar(450) NOT NULL,
        [SKU] nvarchar(450) NOT NULL,
        [ProductName] nvarchar(max) NOT NULL,
        [CurrentStock] int NOT NULL,
        [Threshold] int NOT NULL,
        [Severity] nvarchar(max) NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [IsResolved] bit NOT NULL,
        [ResolvedByUserId] nvarchar(max) NULL,
        [ResolvedAt] datetime2 NULL,
        CONSTRAINT [PK_WarehouseAlerts] PRIMARY KEY ([AlertId])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260928155836_InitialCreate'
)
BEGIN
    CREATE TABLE [OrderLineItem] (
        [OrderId] nvarchar(450) NOT NULL,
        [Id] int NOT NULL IDENTITY,
        [SKU] nvarchar(max) NOT NULL,
        [ProductName] nvarchar(max) NOT NULL,
        [Quantity] int NOT NULL,
        [UnitPrice] decimal(18,2) NOT NULL,
        CONSTRAINT [PK_OrderLineItem] PRIMARY KEY ([OrderId], [Id]),
        CONSTRAINT [FK_OrderLineItem_Orders_OrderId] FOREIGN KEY ([OrderId]) REFERENCES [Orders] ([OrderId]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260928155836_InitialCreate'
)
BEGIN
    CREATE TABLE [Inventory] (
        [InventoryId] nvarchar(450) NOT NULL,
        [SKU] nvarchar(450) NOT NULL,
        [WarehouseLocation] nvarchar(max) NOT NULL,
        [QuantityOnHand] int NOT NULL,
        [QuantityReserved] int NOT NULL,
        [ReorderLevel] int NOT NULL,
        [LastCountedAt] datetime2 NOT NULL,
        [LastRestockedAt] datetime2 NULL,
        CONSTRAINT [PK_Inventory] PRIMARY KEY ([InventoryId]),
        CONSTRAINT [FK_Inventory_Products_SKU] FOREIGN KEY ([SKU]) REFERENCES [Products] ([SKU]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260928155836_InitialCreate'
)
BEGIN
    CREATE TABLE [PurchaseOrderLineItem] (
        [PurchaseOrderId] nvarchar(450) NOT NULL,
        [Id] int NOT NULL IDENTITY,
        [SKU] nvarchar(max) NOT NULL,
        [Quantity] int NOT NULL,
        [UnitCost] decimal(18,2) NOT NULL,
        CONSTRAINT [PK_PurchaseOrderLineItem] PRIMARY KEY ([PurchaseOrderId], [Id]),
        CONSTRAINT [FK_PurchaseOrderLineItem_PurchaseOrders_PurchaseOrderId] FOREIGN KEY ([PurchaseOrderId]) REFERENCES [PurchaseOrders] ([PurchaseOrderId]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260928155836_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Inventory_SKU] ON [Inventory] ([SKU]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260928155836_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Orders_ClientId] ON [Orders] ([ClientId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260928155836_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Orders_Status] ON [Orders] ([Status]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260928155836_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Products_Category] ON [Products] ([Category]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260928155836_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Products_StockQty] ON [Products] ([StockQty]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260928155836_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_WarehouseAlerts_SKU_IsResolved] ON [WarehouseAlerts] ([SKU], [IsResolved]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260928155836_InitialCreate'
)
BEGIN
    CREATE TABLE [Assets] (
        [AssetId] nvarchar(450) NOT NULL,
        [SerialNumber] nvarchar(max) NOT NULL,
        [Model] nvarchar(max) NOT NULL,
        [Manufacturer] nvarchar(max) NOT NULL,
        [ClientId] nvarchar(max) NOT NULL,
        [Location] nvarchar(max) NULL,
        [WarrantyExpiry] datetime2 NULL,
        [LastServiceDate] datetime2 NULL,
        [NextServiceDue] datetime2 NULL,
        [Status] nvarchar(max) NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        CONSTRAINT [PK_Assets] PRIMARY KEY ([AssetId])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260928155836_InitialCreate'
)
BEGIN
    CREATE TABLE [DeliveryLogs] (
        [DeliveryId] nvarchar(450) NOT NULL,
        [TrackingNumber] nvarchar(max) NOT NULL,
        [OrderId] nvarchar(max) NOT NULL,
        [ClientId] nvarchar(max) NOT NULL,
        [Origin] nvarchar(max) NOT NULL,
        [Destination] nvarchar(max) NOT NULL,
        [WeightKg] decimal(18,2) NOT NULL,
        [Carrier] nvarchar(max) NULL,
        [Status] nvarchar(max) NOT NULL,
        [EstimatedArrival] datetime2 NOT NULL,
        [ActualArrival] datetime2 NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        CONSTRAINT [PK_DeliveryLogs] PRIMARY KEY ([DeliveryId])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260928155836_InitialCreate'
)
BEGIN
    CREATE TABLE [Users] (
        [UserId] nvarchar(450) NOT NULL,
        [DisplayName] nvarchar(max) NOT NULL,
        [Email] nvarchar(max) NOT NULL,
        [PasswordHash] nvarchar(max) NOT NULL,
        [Role] nvarchar(max) NOT NULL,
        [IsActive] bit NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [LastLoginAt] datetime2 NULL,
        CONSTRAINT [PK_Users] PRIMARY KEY ([UserId])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260928155836_InitialCreate'
)
BEGIN
    CREATE TABLE [AssetServiceLog] (
        [LogId] nvarchar(450) NOT NULL,
        [AssetId] nvarchar(450) NOT NULL,
        [ServiceDate] datetime2 NOT NULL,
        [PerformedByUserId] nvarchar(max) NOT NULL,
        [Description] nvarchar(max) NOT NULL,
        [Outcome] nvarchar(max) NULL,
        [PartsReplaced] nvarchar(max) NULL,
        CONSTRAINT [PK_AssetServiceLog] PRIMARY KEY ([LogId]),
        CONSTRAINT [FK_AssetServiceLog_Assets_AssetId] FOREIGN KEY ([AssetId]) REFERENCES [Assets] ([AssetId]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260928155836_InitialCreate'
)
BEGIN
    CREATE TABLE [Tickets] (
        [TicketId] nvarchar(450) NOT NULL,
        [TrackingNumber] nvarchar(max) NOT NULL,
        [ClientId] nvarchar(max) NOT NULL,
        [EquipmentId] nvarchar(450) NULL,
        [AssignedToUserId] nvarchar(max) NULL,
        [CreatedByUserId] nvarchar(max) NOT NULL,
        [Title] nvarchar(max) NOT NULL,
        [Description] nvarchar(max) NOT NULL,
        [PhotoUrls] nvarchar(max) NOT NULL,
        [Priority] nvarchar(max) NOT NULL,
        [Status] nvarchar(max) NOT NULL,
        [Category] nvarchar(max) NOT NULL,
        [SlaDeadline] datetime2 NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        CONSTRAINT [PK_Tickets] PRIMARY KEY ([TicketId]),
        CONSTRAINT [FK_Tickets_Assets_EquipmentId] FOREIGN KEY ([EquipmentId]) REFERENCES [Assets] ([AssetId])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260928155836_InitialCreate'
)
BEGIN
    CREATE TABLE [TicketComment] (
        [CommentId] nvarchar(450) NOT NULL,
        [TicketId] nvarchar(450) NOT NULL,
        [UserId] nvarchar(max) NOT NULL,
        [Comment] nvarchar(max) NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [IsInternal] bit NOT NULL,
        CONSTRAINT [PK_TicketComment] PRIMARY KEY ([CommentId]),
        CONSTRAINT [FK_TicketComment_Tickets_TicketId] FOREIGN KEY ([TicketId]) REFERENCES [Tickets] ([TicketId]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260928155836_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_AssetServiceLog_AssetId] ON [AssetServiceLog] ([AssetId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260928155836_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_TicketComment_TicketId] ON [TicketComment] ([TicketId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260928155836_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Tickets_EquipmentId] ON [Tickets] ([EquipmentId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260928155836_InitialCreate'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260928155836_InitialCreate', N'10.0.12');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261002201855_InitialCreate2'
)
BEGIN
    DECLARE @var nvarchar(max);
    SELECT @var = QUOTENAME([d].[name])
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Tickets]') AND [c].[name] = N'ClientId');
    IF @var IS NOT NULL EXEC(N'ALTER TABLE [Tickets] DROP CONSTRAINT ' + @var + ';');
    ALTER TABLE [Tickets] ALTER COLUMN [ClientId] nvarchar(450) NOT NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261002201855_InitialCreate2'
)
BEGIN
    DECLARE @var1 nvarchar(max);
    SELECT @var1 = QUOTENAME([d].[name])
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Assets]') AND [c].[name] = N'ClientId');
    IF @var1 IS NOT NULL EXEC(N'ALTER TABLE [Assets] DROP CONSTRAINT ' + @var1 + ';');
    ALTER TABLE [Assets] ALTER COLUMN [ClientId] nvarchar(450) NOT NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261002201855_InitialCreate2'
)
BEGIN
    CREATE TABLE [Clients] (
        [ClientId] nvarchar(450) NOT NULL,
        [ClientName] nvarchar(max) NOT NULL,
        [Sector] nvarchar(max) NOT NULL,
        [Address] nvarchar(max) NOT NULL,
        [ContactPerson] nvarchar(max) NOT NULL,
        [ContactEmail] nvarchar(max) NOT NULL,
        [ContactPhone] nvarchar(max) NOT NULL,
        [ContractStatus] nvarchar(max) NOT NULL,
        [AssignedRepId] nvarchar(max) NULL,
        [LastContactDate] datetime2 NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        CONSTRAINT [PK_Clients] PRIMARY KEY ([ClientId])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261002201855_InitialCreate2'
)
BEGIN
    CREATE TABLE [Deals] (
        [DealId] nvarchar(450) NOT NULL,
        [InquiryId] nvarchar(max) NULL,
        [ClientId] nvarchar(450) NOT NULL,
        [Stage] nvarchar(max) NOT NULL,
        [Revenue] decimal(18,2) NOT NULL,
        [OwnerUserId] nvarchar(max) NULL,
        [ExpectedCloseDate] datetime2 NULL,
        [Probability] int NOT NULL,
        [Notes] nvarchar(max) NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        CONSTRAINT [PK_Deals] PRIMARY KEY ([DealId]),
        CONSTRAINT [FK_Deals_Clients_ClientId] FOREIGN KEY ([ClientId]) REFERENCES [Clients] ([ClientId]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261002201855_InitialCreate2'
)
BEGIN
    CREATE TABLE [KeyPersonnel] (
        [Id] nvarchar(450) NOT NULL,
        [ClientId] nvarchar(450) NOT NULL,
        [Name] nvarchar(max) NOT NULL,
        [Role] nvarchar(max) NOT NULL,
        [Email] nvarchar(max) NOT NULL,
        [Phone] nvarchar(max) NOT NULL,
        CONSTRAINT [PK_KeyPersonnel] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_KeyPersonnel_Clients_ClientId] FOREIGN KEY ([ClientId]) REFERENCES [Clients] ([ClientId]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261002201855_InitialCreate2'
)
BEGIN
    CREATE TABLE [Quotations] (
        [QuotationId] nvarchar(450) NOT NULL,
        [QuoteReference] nvarchar(max) NOT NULL,
        [DealId] nvarchar(450) NOT NULL,
        [ClientId] nvarchar(450) NOT NULL,
        [DateGenerated] datetime2 NOT NULL,
        [ValidUntil] datetime2 NOT NULL,
        [Subtotal] decimal(18,2) NOT NULL,
        [VatRate] decimal(18,2) NOT NULL,
        [VatAmount] decimal(18,2) NOT NULL,
        [GrandTotal] decimal(18,2) NOT NULL,
        [Terms] nvarchar(max) NOT NULL,
        [Status] nvarchar(max) NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        CONSTRAINT [PK_Quotations] PRIMARY KEY ([QuotationId]),
        CONSTRAINT [FK_Quotations_Clients_ClientId] FOREIGN KEY ([ClientId]) REFERENCES [Clients] ([ClientId]) ON DELETE NO ACTION,
        CONSTRAINT [FK_Quotations_Deals_DealId] FOREIGN KEY ([DealId]) REFERENCES [Deals] ([DealId]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261002201855_InitialCreate2'
)
BEGIN
    CREATE TABLE [QuotationLineItem] (
        [LineItemId] nvarchar(450) NOT NULL,
        [QuotationId] nvarchar(450) NOT NULL,
        [SKU] nvarchar(max) NOT NULL,
        [ProductName] nvarchar(max) NOT NULL,
        [Quantity] int NOT NULL,
        [UnitPrice] decimal(18,2) NOT NULL,
        CONSTRAINT [PK_QuotationLineItem] PRIMARY KEY ([LineItemId]),
        CONSTRAINT [FK_QuotationLineItem_Quotations_QuotationId] FOREIGN KEY ([QuotationId]) REFERENCES [Quotations] ([QuotationId]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261002201855_InitialCreate2'
)
BEGIN
    CREATE INDEX [IX_Tickets_ClientId] ON [Tickets] ([ClientId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261002201855_InitialCreate2'
)
BEGIN
    CREATE INDEX [IX_Assets_ClientId] ON [Assets] ([ClientId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261002201855_InitialCreate2'
)
BEGIN
    CREATE INDEX [IX_Deals_ClientId] ON [Deals] ([ClientId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261002201855_InitialCreate2'
)
BEGIN
    CREATE INDEX [IX_KeyPersonnel_ClientId] ON [KeyPersonnel] ([ClientId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261002201855_InitialCreate2'
)
BEGIN
    CREATE INDEX [IX_QuotationLineItem_QuotationId] ON [QuotationLineItem] ([QuotationId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261002201855_InitialCreate2'
)
BEGIN
    CREATE INDEX [IX_Quotations_ClientId] ON [Quotations] ([ClientId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261002201855_InitialCreate2'
)
BEGIN
    CREATE INDEX [IX_Quotations_DealId] ON [Quotations] ([DealId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261002201855_InitialCreate2'
)
BEGIN
    ALTER TABLE [Assets] ADD CONSTRAINT [FK_Assets_Clients_ClientId] FOREIGN KEY ([ClientId]) REFERENCES [Clients] ([ClientId]) ON DELETE CASCADE;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261002201855_InitialCreate2'
)
BEGIN
    ALTER TABLE [Orders] ADD CONSTRAINT [FK_Orders_Clients_ClientId] FOREIGN KEY ([ClientId]) REFERENCES [Clients] ([ClientId]) ON DELETE CASCADE;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261002201855_InitialCreate2'
)
BEGIN
    ALTER TABLE [Tickets] ADD CONSTRAINT [FK_Tickets_Clients_ClientId] FOREIGN KEY ([ClientId]) REFERENCES [Clients] ([ClientId]) ON DELETE CASCADE;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261002201855_InitialCreate2'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20261002201855_InitialCreate2', N'10.0.12');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261003193000_AddTicketSlaColumns'
)
BEGIN
    ALTER TABLE [Tickets] ADD [EscalatedAt] datetime2 NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261003193000_AddTicketSlaColumns'
)
BEGIN
    ALTER TABLE [Tickets] ADD [EscalationLevel] int NOT NULL DEFAULT 0;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261003193000_AddTicketSlaColumns'
)
BEGIN
    ALTER TABLE [Tickets] ADD [IsSlaBreached] bit NOT NULL DEFAULT CAST(0 AS bit);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261003193000_AddTicketSlaColumns'
)
BEGIN
    ALTER TABLE [Tickets] ADD [ResolutionNotes] nvarchar(max) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261003193000_AddTicketSlaColumns'
)
BEGIN
    ALTER TABLE [Tickets] ADD [ResolvedAt] datetime2 NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261003193000_AddTicketSlaColumns'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20261003193000_AddTicketSlaColumns', N'10.0.12');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261004172006_AddMobileCareTables'
)
BEGIN
    CREATE TABLE [LabResultRecords] (
        [LabResultId] nvarchar(450) NOT NULL,
        [UserId] nvarchar(max) NOT NULL,
        [Reference] nvarchar(max) NOT NULL,
        [TestName] nvarchar(max) NOT NULL,
        [Laboratory] nvarchar(max) NOT NULL,
        [ReportedAt] datetime2 NOT NULL,
        [Status] nvarchar(max) NOT NULL,
        [FileSizeKb] int NULL,
        [IsNew] bit NOT NULL,
        [PdfUrl] nvarchar(max) NULL,
        CONSTRAINT [PK_LabResultRecords] PRIMARY KEY ([LabResultId])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261004172006_AddMobileCareTables'
)
BEGIN
    CREATE TABLE [MobileDevices] (
        [DeviceId] nvarchar(450) NOT NULL,
        [UserId] nvarchar(max) NOT NULL,
        [Name] nvarchar(max) NOT NULL,
        [Type] nvarchar(max) NOT NULL,
        [Status] nvarchar(max) NOT NULL,
        [LastSyncAt] datetime2 NULL,
        [CreatedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_MobileDevices] PRIMARY KEY ([DeviceId])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261004172006_AddMobileCareTables'
)
BEGIN
    CREATE TABLE [MobilePrescriptions] (
        [PrescriptionId] nvarchar(450) NOT NULL,
        [UserId] nvarchar(max) NOT NULL,
        [Reference] nvarchar(max) NOT NULL,
        [Medication] nvarchar(max) NOT NULL,
        [Instructions] nvarchar(max) NOT NULL,
        [Prescriber] nvarchar(max) NOT NULL,
        [IssuedAt] datetime2 NOT NULL,
        [Status] nvarchar(max) NOT NULL,
        [QrPayload] nvarchar(max) NOT NULL,
        CONSTRAINT [PK_MobilePrescriptions] PRIMARY KEY ([PrescriptionId])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261004172006_AddMobileCareTables'
)
BEGIN
    CREATE TABLE [MobileProfiles] (
        [UserId] nvarchar(450) NOT NULL,
        [Phone] nvarchar(max) NOT NULL,
        [Company] nvarchar(max) NOT NULL,
        [VatNumber] nvarchar(max) NULL,
        [AddressName] nvarchar(max) NOT NULL,
        [Street] nvarchar(max) NOT NULL,
        [City] nvarchar(max) NOT NULL,
        [Country] nvarchar(max) NOT NULL,
        [UpdatedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_MobileProfiles] PRIMARY KEY ([UserId])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261004172006_AddMobileCareTables'
)
BEGIN
    CREATE TABLE [MobileSettings] (
        [UserId] nvarchar(450) NOT NULL,
        [PushNotifications] bit NOT NULL,
        [BiometricLogin] bit NOT NULL,
        [DarkMode] bit NOT NULL,
        [UpdatedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_MobileSettings] PRIMARY KEY ([UserId])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261004172006_AddMobileCareTables'
)
BEGIN
    CREATE TABLE [TelehealthAppointments] (
        [AppointmentId] nvarchar(450) NOT NULL,
        [UserId] nvarchar(max) NOT NULL,
        [DoctorName] nvarchar(max) NOT NULL,
        [Specialty] nvarchar(max) NOT NULL,
        [Facility] nvarchar(max) NOT NULL,
        [StartsAt] datetime2 NOT NULL,
        [Status] nvarchar(max) NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_TelehealthAppointments] PRIMARY KEY ([AppointmentId])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261004172006_AddMobileCareTables'
)
BEGIN
    CREATE TABLE [VitalReadings] (
        [ReadingId] nvarchar(450) NOT NULL,
        [UserId] nvarchar(max) NOT NULL,
        [Type] nvarchar(max) NOT NULL,
        [Value] nvarchar(max) NOT NULL,
        [Unit] nvarchar(max) NOT NULL,
        [StatusLabel] nvarchar(max) NOT NULL,
        [InRange] bit NOT NULL,
        [RecordedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_VitalReadings] PRIMARY KEY ([ReadingId])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261004172006_AddMobileCareTables'
)
BEGIN
    CREATE TABLE [WeeklyExerciseLogs] (
        [LogId] nvarchar(450) NOT NULL,
        [UserId] nvarchar(max) NOT NULL,
        [WeekStart] datetime2 NOT NULL,
        [DayIndex] int NOT NULL,
        [Minutes] int NOT NULL,
        CONSTRAINT [PK_WeeklyExerciseLogs] PRIMARY KEY ([LogId])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261004172006_AddMobileCareTables'
)
BEGIN
    CREATE TABLE [WellnessGoals] (
        [GoalId] nvarchar(450) NOT NULL,
        [UserId] nvarchar(max) NOT NULL,
        [Type] nvarchar(max) NOT NULL,
        [Name] nvarchar(max) NOT NULL,
        [Current] float NOT NULL,
        [Target] float NOT NULL,
        [Unit] nvarchar(max) NOT NULL,
        [Date] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_WellnessGoals] PRIMARY KEY ([GoalId])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261004172006_AddMobileCareTables'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20261004172006_AddMobileCareTables', N'10.0.12');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261004212955_AddProductImageUrl'
)
BEGIN
    ALTER TABLE [Products] ADD [ImageUrl] nvarchar(max) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261004212955_AddProductImageUrl'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20261004212955_AddProductImageUrl', N'10.0.12');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261004215038_AddInquiryQueue'
)
BEGIN
    CREATE TABLE [Inquiries] (
        [InquiryId] nvarchar(450) NOT NULL,
        [ClientId] nvarchar(450) NULL,
        [ContactName] nvarchar(max) NOT NULL,
        [ContactEmail] nvarchar(max) NULL,
        [ContactPhone] nvarchar(max) NULL,
        [Company] nvarchar(max) NULL,
        [Subject] nvarchar(max) NOT NULL,
        [Description] nvarchar(max) NULL,
        [Source] nvarchar(max) NOT NULL,
        [Status] nvarchar(max) NOT NULL,
        [Score] int NOT NULL,
        [OwnerUserId] nvarchar(max) NULL,
        [Notes] nvarchar(max) NULL,
        [ResponseDueAt] datetime2 NULL,
        [RespondedAt] datetime2 NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        CONSTRAINT [PK_Inquiries] PRIMARY KEY ([InquiryId]),
        CONSTRAINT [FK_Inquiries_Clients_ClientId] FOREIGN KEY ([ClientId]) REFERENCES [Clients] ([ClientId])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261004215038_AddInquiryQueue'
)
BEGIN
    CREATE INDEX [IX_Inquiries_ClientId] ON [Inquiries] ([ClientId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261004215038_AddInquiryQueue'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20261004215038_AddInquiryQueue', N'10.0.12');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261011171905_AddMobileDeviceExternalId'
)
BEGIN
    ALTER TABLE [MobileDevices] ADD [ExternalId] nvarchar(max) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261011171905_AddMobileDeviceExternalId'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20261011171905_AddMobileDeviceExternalId', N'10.0.12');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261023000000_AddHealthMetricTable'
)
BEGIN
    CREATE TABLE [HealthMetrics] (
        [MetricId] nvarchar(450) NOT NULL,
        [ClientId] nvarchar(max) NOT NULL,
        [DeviceId] nvarchar(max) NOT NULL,
        [MetricType] nvarchar(max) NOT NULL,
        [Value] nvarchar(max) NOT NULL,
        [Unit] nvarchar(max) NOT NULL,
        [Timestamp] datetime2 NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_HealthMetrics] PRIMARY KEY ([MetricId])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261023000000_AddHealthMetricTable'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20261023000000_AddHealthMetricTable', N'10.0.12');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261024150000_AddClientTimelineNotes'
)
BEGIN
    CREATE TABLE [ClientTimelineNotes] (
        [NoteId] nvarchar(450) NOT NULL,
        [ClientId] nvarchar(450) NOT NULL,
        [Text] nvarchar(4000) NOT NULL,
        [CreatedByUserId] nvarchar(max) NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_ClientTimelineNotes] PRIMARY KEY ([NoteId])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261024150000_AddClientTimelineNotes'
)
BEGIN
    CREATE INDEX [IX_ClientTimelineNotes_ClientId_CreatedAt] ON [ClientTimelineNotes] ([ClientId], [CreatedAt]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261024150000_AddClientTimelineNotes'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20261024150000_AddClientTimelineNotes', N'10.0.12');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261025060236_InitialCreateupgrade'
)
BEGIN
    DECLARE @var2 nvarchar(max);
    SELECT @var2 = QUOTENAME([d].[name])
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Users]') AND [c].[name] = N'Role');
    IF @var2 IS NOT NULL EXEC(N'ALTER TABLE [Users] DROP CONSTRAINT ' + @var2 + ';');
    ALTER TABLE [Users] ALTER COLUMN [Role] nvarchar(50) NOT NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261025060236_InitialCreateupgrade'
)
BEGIN
    DECLARE @var3 nvarchar(max);
    SELECT @var3 = QUOTENAME([d].[name])
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Users]') AND [c].[name] = N'Email');
    IF @var3 IS NOT NULL EXEC(N'ALTER TABLE [Users] DROP CONSTRAINT ' + @var3 + ';');
    ALTER TABLE [Users] ALTER COLUMN [Email] nvarchar(200) NOT NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261025060236_InitialCreateupgrade'
)
BEGIN
    DECLARE @var4 nvarchar(max);
    SELECT @var4 = QUOTENAME([d].[name])
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Users]') AND [c].[name] = N'DisplayName');
    IF @var4 IS NOT NULL EXEC(N'ALTER TABLE [Users] DROP CONSTRAINT ' + @var4 + ';');
    ALTER TABLE [Users] ALTER COLUMN [DisplayName] nvarchar(150) NOT NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261025060236_InitialCreateupgrade'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20261025060236_InitialCreateupgrade', N'10.0.12');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261025110000_AddB2BAppointments'
)
BEGIN
    CREATE TABLE [Appointments] (
        [Id] nvarchar(450) NOT NULL,
        [ClientId] nvarchar(450) NOT NULL,
        [RequestedByUserId] nvarchar(450) NOT NULL,
        [Topic] nvarchar(160) NOT NULL,
        [ScheduledDate] datetime2 NOT NULL,
        [Status] nvarchar(24) NOT NULL,
        [Notes] nvarchar(4000) NULL,
        [AssignedToUserId] nvarchar(450) NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        CONSTRAINT [PK_Appointments] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261025110000_AddB2BAppointments'
)
BEGIN
    CREATE INDEX [IX_Appointments_ClientId_ScheduledDate] ON [Appointments] ([ClientId], [ScheduledDate]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261025110000_AddB2BAppointments'
)
BEGIN
    CREATE INDEX [IX_Appointments_Status_ScheduledDate] ON [Appointments] ([Status], [ScheduledDate]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261025110000_AddB2BAppointments'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20261025110000_AddB2BAppointments', N'10.0.12');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261025111000_AddSalesGoals'
)
BEGIN
    CREATE TABLE [SalesGoals] (
        [Id] nvarchar(450) NOT NULL,
        [UserId] nvarchar(450) NOT NULL,
        [Title] nvarchar(160) NOT NULL,
        [Target] decimal(18,2) NOT NULL,
        [Achieved] decimal(18,2) NOT NULL,
        [Period] nvarchar(24) NOT NULL,
        [PeriodLabel] nvarchar(80) NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        CONSTRAINT [PK_SalesGoals] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261025111000_AddSalesGoals'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20261025111000_AddSalesGoals', N'10.0.12');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261025112000_AddUserPasswordResetFields'
)
BEGIN
    ALTER TABLE [Users] ADD [PasswordResetToken] nvarchar(256) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261025112000_AddUserPasswordResetFields'
)
BEGIN
    ALTER TABLE [Users] ADD [PasswordResetExpiry] datetime2 NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261025112000_AddUserPasswordResetFields'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20261025112000_AddUserPasswordResetFields', N'10.0.12');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261025130000_AddAuditLogs'
)
BEGIN
    CREATE TABLE [AuditLogs] (
        [AuditId] nvarchar(450) NOT NULL,
        [EntityType] nvarchar(max) NOT NULL,
        [EntityId] nvarchar(max) NOT NULL,
        [Action] nvarchar(max) NOT NULL,
        [UserId] nvarchar(max) NULL,
        [UserEmail] nvarchar(max) NULL,
        [OldValues] nvarchar(max) NULL,
        [NewValues] nvarchar(max) NULL,
        [IpAddress] nvarchar(max) NULL,
        [Timestamp] datetime2 NOT NULL,
        CONSTRAINT [PK_AuditLogs] PRIMARY KEY ([AuditId])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261025130000_AddAuditLogs'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20261025130000_AddAuditLogs', N'10.0.12');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261025174339_AddAppointmentMeetingLinks'
)
BEGIN
    ALTER TABLE [Appointments] ADD [MeetingUrl] nvarchar(1000) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261025174339_AddAppointmentMeetingLinks'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20261025174339_AddAppointmentMeetingLinks', N'10.0.12');
END;

COMMIT;
GO

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
GO

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
GO

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
GO

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
GO

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
GO

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
GO

