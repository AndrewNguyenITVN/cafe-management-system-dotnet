-- =============================================================
-- CafeManagement - Full Database Schema
-- Chạy script này trên SQL Server để tạo toàn bộ database.
-- Yêu cầu: SQL Server 2019+ hoặc SQL Server 2022 (Docker OK)
-- =============================================================

USE master;
GO

-- Tạo database nếu chưa có
IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = N'CafeManagement')
BEGIN
    CREATE DATABASE CafeManagement;
END
GO

USE CafeManagement;
GO

-- =============================================================
-- PHẦN 1: ASP.NET CORE IDENTITY TABLES
-- (EF Core tạo tự động khi dùng AddIdentity, ta tạo thủ công)
-- =============================================================

IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='AspNetRoles' AND xtype='U')
CREATE TABLE AspNetRoles (
    Id               NVARCHAR(450) NOT NULL PRIMARY KEY,
    Name             NVARCHAR(256) NULL,
    NormalizedName   NVARCHAR(256) NULL,
    ConcurrencyStamp NVARCHAR(MAX) NULL
);
GO

CREATE UNIQUE INDEX RoleNameIndex ON AspNetRoles (NormalizedName)
    WHERE NormalizedName IS NOT NULL;
GO

IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='AspNetRoleClaims' AND xtype='U')
CREATE TABLE AspNetRoleClaims (
    Id         INT IDENTITY NOT NULL PRIMARY KEY,
    RoleId     NVARCHAR(450) NOT NULL,
    ClaimType  NVARCHAR(MAX) NULL,
    ClaimValue NVARCHAR(MAX) NULL,
    CONSTRAINT FK_AspNetRoleClaims_AspNetRoles
        FOREIGN KEY (RoleId) REFERENCES AspNetRoles(Id) ON DELETE CASCADE
);
GO

CREATE INDEX IX_AspNetRoleClaims_RoleId ON AspNetRoleClaims (RoleId);
GO

-- =============================================================
-- PHẦN 2: MASTER DATA TABLES
-- (tạo trước vì AspNetUsers sẽ FK vào đây)
-- =============================================================

IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Stores' AND xtype='U')
CREATE TABLE Stores (
    Id        INT IDENTITY NOT NULL PRIMARY KEY,
    Name      NVARCHAR(200) NOT NULL,
    Address   NVARCHAR(500) NULL,
    Phone     NVARCHAR(20)  NULL,
    OpenTime  TIME NULL,
    CloseTime TIME NULL,
    IsActive  BIT NOT NULL DEFAULT 1
);
GO

IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='JobPositions' AND xtype='U')
CREATE TABLE JobPositions (
    Id           INT IDENTITY NOT NULL PRIMARY KEY,
    PositionName NVARCHAR(100) NOT NULL,
    HourlyRate   DECIMAL(12,2) NOT NULL,
    IsActive     BIT NOT NULL DEFAULT 1
);
GO

-- =============================================================
-- PHẦN 3: ASPNETUSERS (phụ thuộc Stores, JobPositions)
-- =============================================================

IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='AspNetUsers' AND xtype='U')
CREATE TABLE AspNetUsers (
    Id                   NVARCHAR(450)    NOT NULL PRIMARY KEY,
    -- Custom fields
    FullName             NVARCHAR(200)    NOT NULL,
    PositionId           INT              NULL,
    StoreId              INT              NULL,
    PinCode              NVARCHAR(10)     NULL,
    IsActive             BIT              NOT NULL DEFAULT 1,
    -- Identity standard fields
    UserName             NVARCHAR(256)    NULL,
    NormalizedUserName   NVARCHAR(256)    NULL,
    Email                NVARCHAR(256)    NULL,
    NormalizedEmail      NVARCHAR(256)    NULL,
    EmailConfirmed       BIT              NOT NULL DEFAULT 0,
    PasswordHash         NVARCHAR(MAX)    NULL,
    SecurityStamp        NVARCHAR(MAX)    NULL,
    ConcurrencyStamp     NVARCHAR(MAX)    NULL,
    PhoneNumber          NVARCHAR(MAX)    NULL,
    PhoneNumberConfirmed BIT              NOT NULL DEFAULT 0,
    TwoFactorEnabled     BIT              NOT NULL DEFAULT 0,
    LockoutEnd           DATETIMEOFFSET   NULL,
    LockoutEnabled       BIT              NOT NULL DEFAULT 0,
    AccessFailedCount    INT              NOT NULL DEFAULT 0,

    CONSTRAINT FK_AspNetUsers_JobPositions
        FOREIGN KEY (PositionId) REFERENCES JobPositions(Id) ON DELETE SET NULL,
    CONSTRAINT FK_AspNetUsers_Stores
        FOREIGN KEY (StoreId) REFERENCES Stores(Id) ON DELETE SET NULL
);
GO

CREATE INDEX EmailIndex         ON AspNetUsers (NormalizedEmail);
CREATE INDEX IX_AspNetUsers_PositionId ON AspNetUsers (PositionId);
CREATE INDEX IX_AspNetUsers_StoreId    ON AspNetUsers (StoreId);
CREATE UNIQUE INDEX UserNameIndex ON AspNetUsers (NormalizedUserName)
    WHERE NormalizedUserName IS NOT NULL;
GO

IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='AspNetUserRoles' AND xtype='U')
CREATE TABLE AspNetUserRoles (
    UserId NVARCHAR(450) NOT NULL,
    RoleId NVARCHAR(450) NOT NULL,
    CONSTRAINT PK_AspNetUserRoles PRIMARY KEY (UserId, RoleId),
    CONSTRAINT FK_AspNetUserRoles_Users FOREIGN KEY (UserId) REFERENCES AspNetUsers(Id) ON DELETE CASCADE,
    CONSTRAINT FK_AspNetUserRoles_Roles FOREIGN KEY (RoleId) REFERENCES AspNetRoles(Id) ON DELETE CASCADE
);
GO

CREATE INDEX IX_AspNetUserRoles_RoleId ON AspNetUserRoles (RoleId);
GO

IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='AspNetUserClaims' AND xtype='U')
CREATE TABLE AspNetUserClaims (
    Id         INT IDENTITY NOT NULL PRIMARY KEY,
    UserId     NVARCHAR(450) NOT NULL,
    ClaimType  NVARCHAR(MAX) NULL,
    ClaimValue NVARCHAR(MAX) NULL,
    CONSTRAINT FK_AspNetUserClaims_Users FOREIGN KEY (UserId) REFERENCES AspNetUsers(Id) ON DELETE CASCADE
);
GO

CREATE INDEX IX_AspNetUserClaims_UserId ON AspNetUserClaims (UserId);
GO

IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='AspNetUserLogins' AND xtype='U')
CREATE TABLE AspNetUserLogins (
    LoginProvider       NVARCHAR(128) NOT NULL,
    ProviderKey         NVARCHAR(128) NOT NULL,
    ProviderDisplayName NVARCHAR(MAX) NULL,
    UserId              NVARCHAR(450) NOT NULL,
    CONSTRAINT PK_AspNetUserLogins PRIMARY KEY (LoginProvider, ProviderKey),
    CONSTRAINT FK_AspNetUserLogins_Users FOREIGN KEY (UserId) REFERENCES AspNetUsers(Id) ON DELETE CASCADE
);
GO

CREATE INDEX IX_AspNetUserLogins_UserId ON AspNetUserLogins (UserId);
GO

IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='AspNetUserTokens' AND xtype='U')
CREATE TABLE AspNetUserTokens (
    UserId        NVARCHAR(450) NOT NULL,
    LoginProvider NVARCHAR(128) NOT NULL,
    Name          NVARCHAR(128) NOT NULL,
    Value         NVARCHAR(MAX) NULL,
    CONSTRAINT PK_AspNetUserTokens PRIMARY KEY (UserId, LoginProvider, Name),
    CONSTRAINT FK_AspNetUserTokens_Users FOREIGN KEY (UserId) REFERENCES AspNetUsers(Id) ON DELETE CASCADE
);
GO

-- (Không cần __EFMigrationsHistory vì project không dùng EF Migrations)
-- App dùng EF Core chỉ để QUERY/INSERT, không gọi MigrateAsync().

-- =============================================================
-- PHẦN 4: CRM
-- =============================================================

IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Customers' AND xtype='U')
CREATE TABLE Customers (
    Id          INT IDENTITY NOT NULL PRIMARY KEY,
    FullName    NVARCHAR(200) NOT NULL,
    Phone       NVARCHAR(20)  NOT NULL,
    TotalPoints INT NOT NULL DEFAULT 0,
    CreatedAt   DATETIME2 NOT NULL DEFAULT GETDATE()
);
GO

CREATE UNIQUE INDEX UQ_Customers_Phone ON Customers (Phone);
GO

IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='PointHistories' AND xtype='U')
CREATE TABLE PointHistories (
    Id            INT IDENTITY NOT NULL PRIMARY KEY,
    CustomerId    INT NOT NULL,
    OrderId       INT NULL,
    PointsChanged INT NOT NULL,
    Type          NVARCHAR(20) NOT NULL,   -- Earn | Redeem
    CreatedAt     DATETIME2 NOT NULL DEFAULT GETDATE(),
    CONSTRAINT FK_PointHistories_Customers FOREIGN KEY (CustomerId) REFERENCES Customers(Id)
);
GO

CREATE INDEX IX_PointHistories_CustomerId ON PointHistories (CustomerId);
GO

-- =============================================================
-- PHẦN 5: MENU & TOPPINGS
-- =============================================================

IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Categories' AND xtype='U')
CREATE TABLE Categories (
    Id          INT IDENTITY NOT NULL PRIMARY KEY,
    Name        NVARCHAR(100) NOT NULL,
    Description NVARCHAR(500) NULL,
    IsActive    BIT NOT NULL DEFAULT 1
);
GO

IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='MenuItems' AND xtype='U')
CREATE TABLE MenuItems (
    Id          INT IDENTITY NOT NULL PRIMARY KEY,
    CategoryId  INT NOT NULL,
    Name        NVARCHAR(200) NOT NULL,
    Description NVARCHAR(500) NULL,
    BasePrice   DECIMAL(12,0) NOT NULL,
    ImageUrl    NVARCHAR(500) NULL,
    IsActive    BIT NOT NULL DEFAULT 1,
    CONSTRAINT FK_MenuItems_Categories FOREIGN KEY (CategoryId) REFERENCES Categories(Id)
);
GO

CREATE INDEX IX_MenuItems_CategoryId ON MenuItems (CategoryId);
GO

IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Toppings' AND xtype='U')
CREATE TABLE Toppings (
    Id       INT IDENTITY NOT NULL PRIMARY KEY,
    Name     NVARCHAR(100) NOT NULL,
    Price    DECIMAL(12,0) NOT NULL,
    IsActive BIT NOT NULL DEFAULT 1
);
GO

IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='PaymentMethods' AND xtype='U')
CREATE TABLE PaymentMethods (
    Id         INT IDENTITY NOT NULL PRIMARY KEY,
    MethodName NVARCHAR(100) NOT NULL,
    IsActive   BIT NOT NULL DEFAULT 1
);
GO

IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Suppliers' AND xtype='U')
CREATE TABLE Suppliers (
    Id       INT IDENTITY NOT NULL PRIMARY KEY,
    Name     NVARCHAR(200) NOT NULL,
    Phone    NVARCHAR(20)  NULL,
    Address  NVARCHAR(500) NULL,
    IsActive BIT NOT NULL DEFAULT 1
);
GO

-- =============================================================
-- PHẦN 6: POS - ORDERS
-- =============================================================

IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Orders' AND xtype='U')
CREATE TABLE Orders (
    Id             INT IDENTITY NOT NULL PRIMARY KEY,
    StoreId        INT           NOT NULL,
    UserId         NVARCHAR(450) NULL,
    CustomerId     INT           NULL,
    QueueNumber    INT           NOT NULL DEFAULT 0,  -- Số thứ tự ngày, per Store
    OrderDate      DATETIME2     NOT NULL DEFAULT GETDATE(),
    OrderType      NVARCHAR(20)  NOT NULL DEFAULT 'EatIn',   -- EatIn | TakeAway
    TotalAmount    DECIMAL(12,0) NOT NULL DEFAULT 0,
    PointsUsed     INT           NOT NULL DEFAULT 0,
    DiscountAmount DECIMAL(12,0) NOT NULL DEFAULT 0,
    FinalAmount    DECIMAL(12,0) NOT NULL DEFAULT 0,
    Status         INT           NOT NULL DEFAULT 0,
    -- 0: Pending | 1: Processing | 2: Ready | 3: Completed
    CONSTRAINT FK_Orders_Stores    FOREIGN KEY (StoreId)    REFERENCES Stores(Id),
    CONSTRAINT FK_Orders_Users     FOREIGN KEY (UserId)     REFERENCES AspNetUsers(Id) ON DELETE SET NULL,
    CONSTRAINT FK_Orders_Customers FOREIGN KEY (CustomerId) REFERENCES Customers(Id)   ON DELETE SET NULL
);
GO

CREATE INDEX IX_Orders_StoreId_OrderDate ON Orders (StoreId, OrderDate);
CREATE INDEX IX_Orders_UserId            ON Orders (UserId);
CREATE INDEX IX_Orders_CustomerId        ON Orders (CustomerId);
GO

IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='OrderDetails' AND xtype='U')
CREATE TABLE OrderDetails (
    Id         INT IDENTITY NOT NULL PRIMARY KEY,
    OrderId    INT           NOT NULL,
    MenuItemId INT           NOT NULL,
    Quantity   INT           NOT NULL DEFAULT 1,
    UnitPrice  DECIMAL(12,0) NOT NULL,
    Note       NVARCHAR(500) NULL,
    CONSTRAINT FK_OrderDetails_Orders    FOREIGN KEY (OrderId)    REFERENCES Orders(Id)    ON DELETE CASCADE,
    CONSTRAINT FK_OrderDetails_MenuItems FOREIGN KEY (MenuItemId) REFERENCES MenuItems(Id)
);
GO

CREATE INDEX IX_OrderDetails_OrderId    ON OrderDetails (OrderId);
CREATE INDEX IX_OrderDetails_MenuItemId ON OrderDetails (MenuItemId);
GO

IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='OrderDetailToppings' AND xtype='U')
CREATE TABLE OrderDetailToppings (
    Id            INT IDENTITY NOT NULL PRIMARY KEY,
    OrderDetailId INT           NOT NULL,
    ToppingId     INT           NOT NULL,
    Quantity      INT           NOT NULL DEFAULT 1,
    UnitPrice     DECIMAL(12,0) NOT NULL,
    CONSTRAINT FK_OrderDetailToppings_OrderDetails FOREIGN KEY (OrderDetailId) REFERENCES OrderDetails(Id) ON DELETE CASCADE,
    CONSTRAINT FK_OrderDetailToppings_Toppings     FOREIGN KEY (ToppingId)     REFERENCES Toppings(Id)
);
GO

CREATE INDEX IX_OrderDetailToppings_OrderDetailId ON OrderDetailToppings (OrderDetailId);
CREATE INDEX IX_OrderDetailToppings_ToppingId     ON OrderDetailToppings (ToppingId);
GO

IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Transactions' AND xtype='U')
CREATE TABLE Transactions (
    Id              INT IDENTITY NOT NULL PRIMARY KEY,
    OrderId         INT           NOT NULL,
    PaymentMethodId INT           NOT NULL,
    Amount          DECIMAL(12,0) NOT NULL,
    AmountTendered  DECIMAL(12,0) NOT NULL DEFAULT 0,
    ChangeAmount    DECIMAL(12,0) NOT NULL DEFAULT 0,
    TransactionDate DATETIME2     NOT NULL DEFAULT GETDATE(),
    CONSTRAINT FK_Transactions_Orders         FOREIGN KEY (OrderId)         REFERENCES Orders(Id)         ON DELETE CASCADE,
    CONSTRAINT FK_Transactions_PaymentMethods FOREIGN KEY (PaymentMethodId) REFERENCES PaymentMethods(Id)
);
GO

CREATE INDEX IX_Transactions_OrderId         ON Transactions (OrderId);
CREATE INDEX IX_Transactions_PaymentMethodId ON Transactions (PaymentMethodId);
GO

-- FK từ PointHistories đến Orders (tạo sau vì Orders chưa có khi tạo PointHistories)
ALTER TABLE PointHistories
    ADD CONSTRAINT FK_PointHistories_Orders FOREIGN KEY (OrderId) REFERENCES Orders(Id) ON DELETE SET NULL;
GO

CREATE INDEX IX_PointHistories_OrderId ON PointHistories (OrderId);
GO

-- =============================================================
-- PHẦN 7: INVENTORY
-- =============================================================

IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Ingredients' AND xtype='U')
CREATE TABLE Ingredients (
    Id            INT IDENTITY NOT NULL PRIMARY KEY,
    Name          NVARCHAR(200) NOT NULL,
    Unit          NVARCHAR(50)  NOT NULL,
    MinStockLevel DECIMAL(10,2) NOT NULL DEFAULT 0,
    IsActive      BIT NOT NULL DEFAULT 1
);
GO

IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Recipes' AND xtype='U')
CREATE TABLE Recipes (
    Id           INT IDENTITY NOT NULL PRIMARY KEY,
    MenuItemId   INT           NOT NULL,
    IngredientId INT           NOT NULL,
    Quantity     DECIMAL(10,4) NOT NULL,
    WastePercent DECIMAL(5,2)  NOT NULL DEFAULT 0,
    CONSTRAINT FK_Recipes_MenuItems   FOREIGN KEY (MenuItemId)   REFERENCES MenuItems(Id),
    CONSTRAINT FK_Recipes_Ingredients FOREIGN KEY (IngredientId) REFERENCES Ingredients(Id)
);
GO

CREATE INDEX IX_Recipes_MenuItemId   ON Recipes (MenuItemId);
CREATE INDEX IX_Recipes_IngredientId ON Recipes (IngredientId);
GO

IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='InventoryStocks' AND xtype='U')
CREATE TABLE InventoryStocks (
    Id              INT IDENTITY NOT NULL PRIMARY KEY,
    StoreId         INT           NOT NULL,
    IngredientId    INT           NOT NULL,
    CurrentQuantity DECIMAL(10,4) NOT NULL DEFAULT 0,
    CONSTRAINT FK_InventoryStocks_Stores      FOREIGN KEY (StoreId)      REFERENCES Stores(Id),
    CONSTRAINT FK_InventoryStocks_Ingredients FOREIGN KEY (IngredientId) REFERENCES Ingredients(Id),
    CONSTRAINT UQ_InventoryStocks UNIQUE (StoreId, IngredientId)
);
GO

CREATE INDEX IX_InventoryStocks_IngredientId ON InventoryStocks (IngredientId);
GO

IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='InventoryLogs' AND xtype='U')
CREATE TABLE InventoryLogs (
    Id             INT IDENTITY NOT NULL PRIMARY KEY,
    StoreId        INT           NOT NULL,
    IngredientId   INT           NOT NULL,
    ChangeQuantity DECIMAL(10,4) NOT NULL,
    Type           NVARCHAR(30)  NOT NULL,   -- Purchase | Sale | Adjustment | Waste
    ReferenceId    INT           NULL,
    CreatedAt      DATETIME2     NOT NULL DEFAULT GETDATE(),
    CONSTRAINT FK_InventoryLogs_Stores      FOREIGN KEY (StoreId)      REFERENCES Stores(Id),
    CONSTRAINT FK_InventoryLogs_Ingredients FOREIGN KEY (IngredientId) REFERENCES Ingredients(Id)
);
GO

CREATE INDEX IX_InventoryLogs_StoreId_CreatedAt ON InventoryLogs (StoreId, CreatedAt);
CREATE INDEX IX_InventoryLogs_IngredientId      ON InventoryLogs (IngredientId);
GO

IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='PurchaseOrders' AND xtype='U')
CREATE TABLE PurchaseOrders (
    Id         INT IDENTITY NOT NULL PRIMARY KEY,
    StoreId    INT       NOT NULL,
    SupplierId INT       NOT NULL,
    OrderDate  DATETIME2 NOT NULL DEFAULT GETDATE(),
    Status     NVARCHAR(20) NOT NULL DEFAULT 'Received',
    CONSTRAINT FK_PurchaseOrders_Stores    FOREIGN KEY (StoreId)    REFERENCES Stores(Id),
    CONSTRAINT FK_PurchaseOrders_Suppliers FOREIGN KEY (SupplierId) REFERENCES Suppliers(Id)
);
GO

CREATE INDEX IX_PurchaseOrders_StoreId    ON PurchaseOrders (StoreId);
CREATE INDEX IX_PurchaseOrders_SupplierId ON PurchaseOrders (SupplierId);
GO

IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='PurchaseOrderDetails' AND xtype='U')
CREATE TABLE PurchaseOrderDetails (
    Id              INT IDENTITY NOT NULL PRIMARY KEY,
    PurchaseOrderId INT           NOT NULL,
    IngredientId    INT           NOT NULL,
    Quantity        DECIMAL(10,4) NOT NULL,
    CostPrice       DECIMAL(12,2) NOT NULL,
    CONSTRAINT FK_PurchaseOrderDetails_PurchaseOrders FOREIGN KEY (PurchaseOrderId) REFERENCES PurchaseOrders(Id) ON DELETE CASCADE,
    CONSTRAINT FK_PurchaseOrderDetails_Ingredients    FOREIGN KEY (IngredientId)    REFERENCES Ingredients(Id)
);
GO

CREATE INDEX IX_PurchaseOrderDetails_PurchaseOrderId ON PurchaseOrderDetails (PurchaseOrderId);
CREATE INDEX IX_PurchaseOrderDetails_IngredientId    ON PurchaseOrderDetails (IngredientId);
GO

-- =============================================================
-- PHẦN 8: HRM
-- =============================================================

IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Shifts' AND xtype='U')
CREATE TABLE Shifts (
    Id        INT IDENTITY NOT NULL PRIMARY KEY,
    ShiftName NVARCHAR(50) NOT NULL,
    StartTime TIME         NOT NULL,
    EndTime   TIME         NOT NULL
);
GO

IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Schedules' AND xtype='U')
CREATE TABLE Schedules (
    Id       INT IDENTITY NOT NULL PRIMARY KEY,
    UserId   NVARCHAR(450) NOT NULL,
    StoreId  INT           NOT NULL,
    ShiftId  INT           NOT NULL,
    WorkDate DATE          NOT NULL,
    CONSTRAINT FK_Schedules_Users  FOREIGN KEY (UserId)  REFERENCES AspNetUsers(Id) ON DELETE CASCADE,
    CONSTRAINT FK_Schedules_Stores FOREIGN KEY (StoreId) REFERENCES Stores(Id),
    CONSTRAINT FK_Schedules_Shifts FOREIGN KEY (ShiftId) REFERENCES Shifts(Id)
);
GO

CREATE INDEX IX_Schedules_UserId  ON Schedules (UserId);
CREATE INDEX IX_Schedules_StoreId ON Schedules (StoreId);
CREATE INDEX IX_Schedules_ShiftId ON Schedules (ShiftId);
GO

IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Timekeepings' AND xtype='U')
CREATE TABLE Timekeepings (
    Id               INT IDENTITY NOT NULL PRIMARY KEY,
    UserId           NVARCHAR(450) NOT NULL,
    StoreId          INT           NOT NULL,
    Date             DATE          NOT NULL,
    CheckInTime      DATETIME2     NOT NULL,
    CheckOutTime     DATETIME2     NULL,
    TotalHours       DECIMAL(5,2)  NULL,
    HourlyRateAtTime DECIMAL(12,2) NULL,
    IsLate           BIT           NOT NULL DEFAULT 0,
    CONSTRAINT FK_Timekeepings_Users  FOREIGN KEY (UserId)  REFERENCES AspNetUsers(Id) ON DELETE CASCADE,
    CONSTRAINT FK_Timekeepings_Stores FOREIGN KEY (StoreId) REFERENCES Stores(Id)
);
GO

CREATE INDEX IX_Timekeepings_UserId_Date ON Timekeepings (UserId, Date);
CREATE INDEX IX_Timekeepings_StoreId     ON Timekeepings (StoreId);
GO

IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='ShiftHandovers' AND xtype='U')
CREATE TABLE ShiftHandovers (
    Id                INT IDENTITY NOT NULL PRIMARY KEY,
    StoreId           INT           NOT NULL,
    ShiftId           INT           NOT NULL,
    HandoverDate      DATE          NOT NULL,
    OpeningCash       DECIMAL(12,0) NOT NULL DEFAULT 0,
    ExpectedCash      DECIMAL(12,0) NOT NULL DEFAULT 0,
    ActualCashCounted DECIMAL(12,0) NOT NULL DEFAULT 0,
    Difference        DECIMAL(12,0) NOT NULL DEFAULT 0,
    Note              NVARCHAR(500) NULL,
    ConfirmedAt       DATETIME2     NOT NULL DEFAULT GETDATE(),
    ConfirmedByUserId NVARCHAR(450) NULL,
    CONSTRAINT FK_ShiftHandovers_Stores FOREIGN KEY (StoreId) REFERENCES Stores(Id),
    CONSTRAINT FK_ShiftHandovers_Shifts FOREIGN KEY (ShiftId) REFERENCES Shifts(Id)
);
GO

PRINT 'Schema created successfully. Run CafeManagement_SeedData.sql next.';
GO
