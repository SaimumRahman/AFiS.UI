-- =============================================
-- Sales POS - Exchange / Return feature
-- Database changes required for the return & exchange flow
-- Target: AFIS_MAH_LIVE (idempotent - safe to re-run)
-- =============================================

-- 1) SalesMaster: track the exchange credit applied against a new sale
IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'SalesMaster' AND COLUMN_NAME = 'ExchangeAmount')
BEGIN
    ALTER TABLE SalesMaster ADD ExchangeAmount DECIMAL(18,2) NULL;
END
GO

-- 2) SalesMaster: the original invoice whose items are being returned
IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'SalesMaster' AND COLUMN_NAME = 'ReturnInvoiceNo')
BEGIN
    ALTER TABLE SalesMaster ADD ReturnInvoiceNo VARCHAR(50) NULL;
END
GO

-- 3) SalesMaster: flag that this invoice (if set) has been sourced as a return/exchange
IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'SalesMaster' AND COLUMN_NAME = 'IsReturnExchange')
BEGIN
    ALTER TABLE SalesMaster ADD IsReturnExchange BIT NULL;
END
GO

-- 4) SalesDetails: cumulative quantity already returned for each line.
--    Used to cap further returns (available = Qty - ReturnedQty) and to
--    prevent the same line being exchanged more than once.
IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'SalesDetails' AND COLUMN_NAME = 'ReturnedQty')
BEGIN
    ALTER TABLE SalesDetails ADD ReturnedQty DECIMAL(18,2) NOT NULL CONSTRAINT DF_SalesDetails_ReturnedQty DEFAULT 0;
END
GO

-- Optional: verify applied columns
-- SELECT TABLE_NAME, COLUMN_NAME, DATA_TYPE
-- FROM INFORMATION_SCHEMA.COLUMNS
-- WHERE (TABLE_NAME = 'SalesMaster'  AND COLUMN_NAME IN ('ExchangeAmount','ReturnInvoiceNo','IsReturnExchange'))
--    OR (TABLE_NAME = 'SalesDetails' AND COLUMN_NAME = 'ReturnedQty');