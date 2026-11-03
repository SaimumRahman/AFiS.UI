-- Monthly invoice-number sequences.
-- Run once against AFIS_MAH_LIVE.
IF COL_LENGTH('InvoiceNumberSequence','Month') IS NULL
    ALTER TABLE InvoiceNumberSequence ADD Month INT NOT NULL CONSTRAINT DF_InvoiceNumberSequence_Month DEFAULT (0);

IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'UQ_InvoiceNumberSequence' AND object_id = OBJECT_ID('InvoiceNumberSequence'))
    ALTER TABLE InvoiceNumberSequence DROP CONSTRAINT UQ_InvoiceNumberSequence;

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'UQ_InvoiceNumberSequence' AND object_id = OBJECT_ID('InvoiceNumberSequence'))
    ALTER TABLE InvoiceNumberSequence ADD CONSTRAINT UQ_InvoiceNumberSequence UNIQUE (StoreId, InvoiceType, Year, Month);
GO