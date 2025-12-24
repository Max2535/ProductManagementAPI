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
CREATE TABLE [Categories] (
    [Id] uniqueidentifier NOT NULL,
    [Name] nvarchar(100) NOT NULL,
    [Description] nvarchar(500) NOT NULL,
    [Slug] nvarchar(100) NOT NULL,
    [IsActive] bit NOT NULL,
    [ParentCategoryId] uniqueidentifier NULL,
    [CreatedAt] datetime2 NOT NULL,
    [CreatedBy] nvarchar(max) NOT NULL,
    [UpdatedAt] datetime2 NULL,
    [UpdatedBy] nvarchar(max) NULL,
    [IsDeleted] bit NOT NULL,
    [DeletedAt] datetime2 NULL,
    [DeletedBy] nvarchar(max) NULL,
    CONSTRAINT [PK_Categories] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Categories_Categories_ParentCategoryId] FOREIGN KEY ([ParentCategoryId]) REFERENCES [Categories] ([Id]) ON DELETE NO ACTION
);

CREATE TABLE [Products] (
    [Id] uniqueidentifier NOT NULL,
    [Name] nvarchar(200) NOT NULL,
    [Description] nvarchar(2000) NOT NULL,
    [SKU] nvarchar(50) NOT NULL,
    [Price] decimal(18,2) NOT NULL,
    [DiscountPrice] decimal(18,2) NULL,
    [StockQuantity] int NOT NULL,
    [MinimumStockLevel] int NOT NULL,
    [Status] nvarchar(20) NOT NULL,
    [CategoryId] uniqueidentifier NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    [CreatedBy] nvarchar(max) NOT NULL,
    [UpdatedAt] datetime2 NULL,
    [UpdatedBy] nvarchar(max) NULL,
    [IsDeleted] bit NOT NULL,
    [DeletedAt] datetime2 NULL,
    [DeletedBy] nvarchar(max) NULL,
    CONSTRAINT [PK_Products] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Products_Categories_CategoryId] FOREIGN KEY ([CategoryId]) REFERENCES [Categories] ([Id]) ON DELETE NO ACTION
);

CREATE TABLE [ProductImages] (
    [Id] uniqueidentifier NOT NULL,
    [ProductId] uniqueidentifier NOT NULL,
    [ImageUrl] nvarchar(max) NOT NULL,
    [IsPrimary] bit NOT NULL,
    [DisplayOrder] int NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    [CreatedBy] nvarchar(max) NOT NULL,
    [UpdatedAt] datetime2 NULL,
    [UpdatedBy] nvarchar(max) NULL,
    [IsDeleted] bit NOT NULL,
    [DeletedAt] datetime2 NULL,
    [DeletedBy] nvarchar(max) NULL,
    CONSTRAINT [PK_ProductImages] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_ProductImages_Products_ProductId] FOREIGN KEY ([ProductId]) REFERENCES [Products] ([Id]) ON DELETE CASCADE
);

CREATE TABLE [Reviews] (
    [Id] uniqueidentifier NOT NULL,
    [ProductId] uniqueidentifier NOT NULL,
    [UserId] nvarchar(max) NOT NULL,
    [UserName] nvarchar(max) NOT NULL,
    [Rating] int NOT NULL,
    [Title] nvarchar(max) NOT NULL,
    [Comment] nvarchar(max) NOT NULL,
    [IsVerifiedPurchase] bit NOT NULL,
    [Status] int NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    [CreatedBy] nvarchar(max) NOT NULL,
    [UpdatedAt] datetime2 NULL,
    [UpdatedBy] nvarchar(max) NULL,
    [IsDeleted] bit NOT NULL,
    [DeletedAt] datetime2 NULL,
    [DeletedBy] nvarchar(max) NULL,
    CONSTRAINT [PK_Reviews] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Reviews_Products_ProductId] FOREIGN KEY ([ProductId]) REFERENCES [Products] ([Id]) ON DELETE CASCADE
);

IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'CreatedAt', N'CreatedBy', N'DeletedAt', N'DeletedBy', N'Description', N'IsActive', N'IsDeleted', N'Name', N'ParentCategoryId', N'Slug', N'UpdatedAt', N'UpdatedBy') AND [object_id] = OBJECT_ID(N'[Categories]'))
    SET IDENTITY_INSERT [Categories] ON;
INSERT INTO [Categories] ([Id], [CreatedAt], [CreatedBy], [DeletedAt], [DeletedBy], [Description], [IsActive], [IsDeleted], [Name], [ParentCategoryId], [Slug], [UpdatedAt], [UpdatedBy])
VALUES ('11111111-1111-1111-1111-111111111111', '2025-12-24T14:46:30.8619829Z', N'System', NULL, NULL, N'Electronic devices and accessories', CAST(1 AS bit), CAST(0 AS bit), N'Electronics', NULL, N'electronics', NULL, NULL);
IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'CreatedAt', N'CreatedBy', N'DeletedAt', N'DeletedBy', N'Description', N'IsActive', N'IsDeleted', N'Name', N'ParentCategoryId', N'Slug', N'UpdatedAt', N'UpdatedBy') AND [object_id] = OBJECT_ID(N'[Categories]'))
    SET IDENTITY_INSERT [Categories] OFF;

IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'CategoryId', N'CreatedAt', N'CreatedBy', N'DeletedAt', N'DeletedBy', N'Description', N'DiscountPrice', N'IsDeleted', N'MinimumStockLevel', N'Name', N'Price', N'SKU', N'Status', N'StockQuantity', N'UpdatedAt', N'UpdatedBy') AND [object_id] = OBJECT_ID(N'[Products]'))
    SET IDENTITY_INSERT [Products] ON;
INSERT INTO [Products] ([Id], [CategoryId], [CreatedAt], [CreatedBy], [DeletedAt], [DeletedBy], [Description], [DiscountPrice], [IsDeleted], [MinimumStockLevel], [Name], [Price], [SKU], [Status], [StockQuantity], [UpdatedAt], [UpdatedBy])
VALUES ('22222222-2222-2222-2222-222222222222', '11111111-1111-1111-1111-111111111111', '2025-12-24T14:46:30.8692613Z', N'System', NULL, NULL, N'This is a sample product', NULL, CAST(0 AS bit), 10, N'Sample Product', 99.99, N'SAMPLE-001', N'Active', 100, NULL, NULL);
IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'CategoryId', N'CreatedAt', N'CreatedBy', N'DeletedAt', N'DeletedBy', N'Description', N'DiscountPrice', N'IsDeleted', N'MinimumStockLevel', N'Name', N'Price', N'SKU', N'Status', N'StockQuantity', N'UpdatedAt', N'UpdatedBy') AND [object_id] = OBJECT_ID(N'[Products]'))
    SET IDENTITY_INSERT [Products] OFF;

CREATE INDEX [IX_Categories_Name] ON [Categories] ([Name]);

CREATE INDEX [IX_Categories_ParentCategoryId] ON [Categories] ([ParentCategoryId]);

CREATE UNIQUE INDEX [IX_Categories_Slug] ON [Categories] ([Slug]);

CREATE INDEX [IX_ProductImages_ProductId] ON [ProductImages] ([ProductId]);

CREATE INDEX [IX_Products_CategoryId] ON [Products] ([CategoryId]);

CREATE INDEX [IX_Products_CategoryId_Status_Price] ON [Products] ([CategoryId], [Status], [Price]);

CREATE INDEX [IX_Products_CreatedAt] ON [Products] ([CreatedAt]);

CREATE INDEX [IX_Products_Name] ON [Products] ([Name]);

CREATE UNIQUE INDEX [IX_Products_SKU] ON [Products] ([SKU]);

CREATE INDEX [IX_Products_Status] ON [Products] ([Status]);

CREATE INDEX [IX_Reviews_ProductId] ON [Reviews] ([ProductId]);

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20251224144632_InitialCreate', N'9.0.8');

UPDATE [Categories] SET [CreatedAt] = '2024-01-01T00:00:00.0000000Z'
WHERE [Id] = '11111111-1111-1111-1111-111111111111';
SELECT @@ROWCOUNT;


UPDATE [Products] SET [CreatedAt] = '2024-01-01T00:00:00.0000000Z'
WHERE [Id] = '22222222-2222-2222-2222-222222222222';
SELECT @@ROWCOUNT;


INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20251224144915_FixSeedDataTimestamps', N'9.0.8');

COMMIT;
GO

