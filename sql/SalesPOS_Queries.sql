-- =============================================
-- Sales POS - Dapper Queries for API Endpoints
-- =============================================

-- Table: SalesMaster
CREATE TABLE SalesMaster (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    InvoiceNo NVARCHAR(50) NOT NULL,
    CustomerId INT NULL,
    CustomerName NVARCHAR(200) NULL,
    CustomerPhone NVARCHAR(50) NULL,
    CustomerAddress NVARCHAR(500) NULL,
    SalesDate DATETIME2 NOT NULL DEFAULT GETDATE(),
    SubTotal DECIMAL(18,2) NOT NULL DEFAULT 0,
    CampaignDiscount DECIMAL(18,2) NULL DEFAULT 0,
    MembershipDiscount DECIMAL(18,2) NULL DEFAULT 0,
    InvoiceDiscount DECIMAL(18,2) NULL DEFAULT 0,
    InvoiceDiscountType NVARCHAR(20) NULL DEFAULT 'Percentage',
    VatAmount DECIMAL(18,2) NULL DEFAULT 0,
    VatPercentage DECIMAL(5,2) NULL DEFAULT 5,
    ExchangeAmount DECIMAL(18,2) NULL DEFAULT 0,
    RoundingAmount DECIMAL(18,2) NULL DEFAULT 0,
    NetAmount DECIMAL(18,2) NOT NULL DEFAULT 0,
    PaidAmount DECIMAL(18,2) NULL DEFAULT 0,
    DueAmount DECIMAL(18,2) NULL DEFAULT 0,
    PaymentStatus NVARCHAR(20) NULL DEFAULT 'Due',
    SalesType NVARCHAR(20) NULL DEFAULT 'Sale',
    Remarks NVARCHAR(500) NULL,
    SalesPersonId INT NULL,
    SalesPersonName NVARCHAR(200) NULL,
    ShiftId INT NULL,
    StoreId INT NULL,
    StoreName NVARCHAR(200) NULL,
    IsActive BIT NOT NULL DEFAULT 1,
    ReturnInvoiceNo NVARCHAR(50) NULL,
    IsReturnExchange BIT NOT NULL DEFAULT 0,
    MembershipTypeId INT NULL,
    MembershipTypeName NVARCHAR(100) NULL,
    DiscountRate INT NULL,
    CreatedBy INT NULL,
    CreatedDate DATETIME2 NOT NULL DEFAULT GETDATE(),
    LastModifiedBy INT NULL,
    LastModifiedDate DATETIME2 NULL,

    CONSTRAINT UQ_SalesMaster_InvoiceNo UNIQUE (InvoiceNo)
);

-- Table: SalesDetails
CREATE TABLE SalesDetails (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    SaleMasterId INT NOT NULL,
    ItemId INT NOT NULL,
    ItemName NVARCHAR(500) NULL,
    Barcode NVARCHAR(100) NULL,
    Quantity DECIMAL(18,2) NOT NULL DEFAULT 1,
    SalePrice DECIMAL(18,2) NOT NULL,
    TotalPrice DECIMAL(18,2) NOT NULL,
    DiscountAmount DECIMAL(18,2) NULL DEFAULT 0,
    ColorId INT NULL,
    ColorName NVARCHAR(100) NULL,
    SizeId INT NULL,
    SizeName NVARCHAR(100) NULL,
    StoreId INT NULL,
    ImagePath NVARCHAR(500) NULL,
    IsActive BIT NOT NULL DEFAULT 1,

    CONSTRAINT FK_SalesDetails_SalesMaster FOREIGN KEY (SaleMasterId) REFERENCES SalesMaster(Id)
);

-- Table: PaymentTransaction
CREATE TABLE PaymentTransaction (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    SaleMasterId INT NOT NULL,
    PaymentType NVARCHAR(20) NOT NULL, -- Cash, MFS, Card
    Amount DECIMAL(18,2) NOT NULL,
    TransactionId NVARCHAR(100) NULL,
    ReferenceNo NVARCHAR(100) NULL,
    PaymentDate DATETIME2 NOT NULL DEFAULT GETDATE(),
    IsActive BIT NOT NULL DEFAULT 1,

    CONSTRAINT FK_PaymentTransaction_SalesMaster FOREIGN KEY (SaleMasterId) REFERENCES SalesMaster(Id)
);

-- =============================================
-- DAPPER QUERIES
-- =============================================

-- 1. Get All Sales (Summary)
-- @params: none
SELECT sm.Id, sm.InvoiceNo, sm.CustomerName, sm.CustomerPhone, sm.SalesDate,
       sm.NetAmount, sm.PaidAmount, sm.DueAmount, sm.PaymentStatus, sm.SalesType,
       sm.SalesPersonName,
       (SELECT COUNT(*) FROM SalesDetails WHERE SaleMasterId = sm.Id) AS TotalItems
FROM SalesMaster sm
WHERE sm.IsActive = 1
ORDER BY sm.Id DESC

-- 2. Get Sale By Id
-- @params: Id int
SELECT * FROM SalesMaster WHERE Id = @Id AND IsActive = 1;
SELECT * FROM SalesDetails WHERE SaleMasterId = @Id AND IsActive = 1;
SELECT * FROM PaymentTransaction WHERE SaleMasterId = @Id AND IsActive = 1;

-- 3. Insert/Update Sale (run in transaction)
-- @params: set of DTO properties

-- INSERT new SalesMaster:
INSERT INTO SalesMaster (
    InvoiceNo, CustomerId, CustomerName, CustomerPhone, CustomerAddress,
    SalesDate, SubTotal, CampaignDiscount, MembershipDiscount, InvoiceDiscount,
    InvoiceDiscountType, VatAmount, VatPercentage, ExchangeAmount, RoundingAmount,
    NetAmount, PaidAmount, DueAmount, PaymentStatus, SalesType, Remarks,
    SalesPersonId, SalesPersonName, ShiftId, StoreId, StoreName,
    IsActive, ReturnInvoiceNo, IsReturnExchange, MembershipTypeId,
    MembershipTypeName, DiscountRate, CreatedBy, CreatedDate
) VALUES (
    @InvoiceNo, @CustomerId, @CustomerName, @CustomerPhone, @CustomerAddress,
    @SalesDate, @SubTotal, @CampaignDiscount, @MembershipDiscount, @InvoiceDiscount,
    @InvoiceDiscountType, @VatAmount, @VatPercentage, @ExchangeAmount, @RoundingAmount,
    @NetAmount, @PaidAmount, @DueAmount, @PaymentStatus, @SalesType, @Remarks,
    @SalesPersonId, @SalesPersonName, @ShiftId, @StoreId, @StoreName,
    1, @ReturnInvoiceNo, @IsReturnExchange, @MembershipTypeId,
    @MembershipTypeName, @DiscountRate, @CreatedBy, GETDATE()
);
SELECT CAST(SCOPE_IDENTITY() AS INT);

-- UPDATE existing SalesMaster:
UPDATE SalesMaster SET
    CustomerId = @CustomerId, CustomerName = @CustomerName, CustomerPhone = @CustomerPhone,
    CustomerAddress = @CustomerAddress, SalesDate = @SalesDate, SubTotal = @SubTotal,
    CampaignDiscount = @CampaignDiscount, MembershipDiscount = @MembershipDiscount,
    InvoiceDiscount = @InvoiceDiscount, InvoiceDiscountType = @InvoiceDiscountType,
    VatAmount = @VatAmount, VatPercentage = @VatPercentage, ExchangeAmount = @ExchangeAmount,
    RoundingAmount = @RoundingAmount, NetAmount = @NetAmount, PaidAmount = @PaidAmount,
    DueAmount = @DueAmount, PaymentStatus = @PaymentStatus, SalesType = @SalesType,
    Remarks = @Remarks, SalesPersonId = @SalesPersonId, SalesPersonName = @SalesPersonName,
    ShiftId = @ShiftId, StoreId = @StoreId, StoreName = @StoreName,
    ReturnInvoiceNo = @ReturnInvoiceNo, IsReturnExchange = @IsReturnExchange,
    MembershipTypeId = @MembershipTypeId, MembershipTypeName = @MembershipTypeName,
    DiscountRate = @DiscountRate, LastModifiedBy = @LastModifiedBy, LastModifiedDate = GETDATE()
WHERE Id = @Id;

-- Delete old details before re-inserting (when updating):
DELETE FROM PaymentTransaction WHERE SaleMasterId = @SaleMasterId;
DELETE FROM SalesDetails WHERE SaleMasterId = @SaleMasterId;

-- Insert SalesDetails:
INSERT INTO SalesDetails (
    SaleMasterId, ItemId, ItemName, Barcode, Quantity, SalePrice,
    TotalPrice, DiscountAmount, ColorId, ColorName, SizeId, SizeName,
    StoreId, ImagePath, IsActive
) VALUES (
    @SaleMasterId, @ItemId, @ItemName, @Barcode, @Quantity, @SalePrice,
    @TotalPrice, @DiscountAmount, @ColorId, @ColorName, @SizeId, @SizeName,
    @StoreId, @ImagePath, 1
);

-- Insert PaymentTransactions:
INSERT INTO PaymentTransaction (
    SaleMasterId, PaymentType, Amount, TransactionId, ReferenceNo, PaymentDate, IsActive
) VALUES (
    @SaleMasterId, @PaymentType, @Amount, @TransactionId, @ReferenceNo, GETDATE(), 1
);

-- 4. Delete Sale (Soft Delete)
-- @params: Id int
UPDATE SalesMaster SET IsActive = 0 WHERE Id = @Id;
UPDATE SalesDetails SET IsActive = 0 WHERE SaleMasterId = @Id;

-- 5. Get Sales By Date Range
-- @params: fromDate datetime2, toDate datetime2
SELECT sm.Id, sm.InvoiceNo, sm.CustomerName, sm.CustomerPhone, sm.SalesDate,
       sm.NetAmount, sm.PaidAmount, sm.DueAmount, sm.PaymentStatus, sm.SalesType,
       sm.SalesPersonName,
       (SELECT COUNT(*) FROM SalesDetails WHERE SaleMasterId = sm.Id) AS TotalItems
FROM SalesMaster sm
WHERE CAST(sm.SalesDate AS DATE) BETWEEN @fromDate AND @toDate
  AND sm.IsActive = 1
ORDER BY sm.Id DESC

-- 6. Get Sales By CustomerId
-- @params: customerId int
SELECT sm.Id, sm.InvoiceNo, sm.CustomerName, sm.CustomerPhone, sm.SalesDate,
       sm.NetAmount, sm.PaidAmount, sm.DueAmount, sm.PaymentStatus, sm.SalesType,
       sm.SalesPersonName,
       (SELECT COUNT(*) FROM SalesDetails WHERE SaleMasterId = sm.Id) AS TotalItems
FROM SalesMaster sm
WHERE sm.CustomerId = @customerId AND sm.IsActive = 1
ORDER BY sm.SalesDate DESC

-- 7. Get Sale By InvoiceNo
-- @params: invoiceNo nvarchar(50)
SELECT * FROM SalesMaster WHERE InvoiceNo = @invoiceNo AND IsActive = 1;
SELECT * FROM SalesDetails WHERE SaleMasterId = (SELECT Id FROM SalesMaster WHERE InvoiceNo = @invoiceNo) AND IsActive = 1;
SELECT * FROM PaymentTransaction WHERE SaleMasterId = (SELECT Id FROM SalesMaster WHERE InvoiceNo = @invoiceNo) AND IsActive = 1;

-- 8. Get New Invoice Number
-- @params: none
SELECT 'INV-' + FORMAT(GETDATE(), 'yyyyMMdd-') + 
       RIGHT('00000' + CAST(ISNULL(MAX(CAST(
           SUBSTRING(InvoiceNo, LEN('INV-' + FORMAT(GETDATE(), 'yyyyMMdd-')) + 1, 5) AS INT
       )), 0) + 1 AS NVARCHAR(5)), 5)
FROM SalesMaster 
WHERE InvoiceNo LIKE 'INV-' + FORMAT(GETDATE(), 'yyyyMMdd-') + '%'

-- 9. Search Products (by name or barcode)
-- @params: term nvarchar(200), storeId int
SELECT i.Id AS ItemId, i.Name AS ItemName, i.Barcode, i.SalePrice,
       s.Quantity AS StockQuantity
FROM Items i
LEFT JOIN Stock s ON s.ItemId = i.Id AND s.StoreId = @storeId
WHERE (i.Name LIKE '%' + @term + '%' OR i.Barcode LIKE '%' + @term + '%')
  AND i.IsActive = 1

-- 10. Search By Barcode
-- @params: barcode nvarchar(100)
SELECT i.Id AS ItemId, i.Name AS ItemName, i.Barcode, i.SalePrice,
       s.Quantity AS StockQuantity
FROM Items i
LEFT JOIN Stock s ON s.ItemId = i.Id
WHERE i.Barcode = @barcode AND i.IsActive = 1
