/* =========================================================================
   Treasury_Tables_DBChanges.sql
   -------------------------------------------------------------------------
   New tables for the accounts data-entry pages (Contra, POS/MFS Settlement,
   Cash Swap, Branch Loan, Supplier Payment split).
   Idempotent: safe to run multiple times. Does NOT touch existing data.
   Target: AFiSDb
   ========================================================================= */
SET NOCOUNT ON;
SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
SET XACT_ABORT ON;
GO

/* ---- 1. SupplierPaymentLines (split supplier payments) ------------------ */
IF OBJECT_ID(N'dbo.SupplierPaymentLines', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.SupplierPaymentLines
    (
        PaymentLineId      INT           NOT NULL IDENTITY(1,1) PRIMARY KEY,
        SupplierPaymentId  INT           NOT NULL,
        FinancialAccountId INT           NULL,
        LedgerCode         NVARCHAR(20)  NOT NULL,
        Amount             DECIMAL(18,2) NOT NULL CHECK (Amount > 0),
        TransactionNo      NVARCHAR(100) NULL,
        CreatedDate        DATETIME      NOT NULL DEFAULT GETDATE(),
        CONSTRAINT FK_SupplierPaymentLines_SupplierPayment
            FOREIGN KEY (SupplierPaymentId) REFERENCES dbo.SupplierPayments (Id) ON DELETE CASCADE,
        CONSTRAINT FK_SupplierPaymentLines_FinancialAccount
            FOREIGN KEY (FinancialAccountId) REFERENCES dbo.FinancialAccounts (Id)
    );
    CREATE INDEX IX_SupplierPaymentLines_Payment ON dbo.SupplierPaymentLines (SupplierPaymentId);
    PRINT 'Table SupplierPaymentLines created.';
END
ELSE
    PRINT 'Table SupplierPaymentLines already exists - no change.';
GO

/* ---- 2. AccSettlement (POS / MFS settlement to bank) ------------------- */
IF OBJECT_ID(N'dbo.AccSettlement', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.AccSettlement
    (
        SettlementId               INT           NOT NULL IDENTITY(1,1) PRIMARY KEY,
        PaymentTransactionId       INT           NOT NULL,
        StoreId                    INT           NOT NULL,
        BranchCode                 NVARCHAR(20)  NOT NULL,
        InvoiceNo                  NVARCHAR(50)  NULL,
        TransactionDate            DATE          NOT NULL,
        ChannelFinancialAccountId   INT           NOT NULL,
        ChannelLedgerCode          NVARCHAR(20)  NOT NULL,
        SettlementAmount           DECIMAL(18,2) NOT NULL CHECK (SettlementAmount > 0),
        TransferAmount             DECIMAL(18,2) NOT NULL CHECK (TransferAmount >= 0),
        CommissionAmount           DECIMAL(18,2) NOT NULL CHECK (CommissionAmount >= 0),
        ReceivingFinancialAccountId INT           NOT NULL,
        ReceivingLedgerCode        NVARCHAR(20)  NOT NULL,
        CommissionLedgerCode       NVARCHAR(20)  NOT NULL,
        EntryId                    BIGINT        NULL,
        Status                     NVARCHAR(20)  NOT NULL DEFAULT 'POSTED' CHECK (Status IN ('POSTED','REVERSED')),
        CreatedBy                  INT           NOT NULL,
        CreatedDate                DATETIME      NOT NULL DEFAULT GETDATE(),
        PostedBy                   INT           NULL,
        PostedDate                 DATETIME      NULL,
        -- NOTE: PaymentTransaction's primary key is PayTranId (not PaymentTransactionId);
        -- the FK must point at that column.
        CONSTRAINT FK_AccSettlement_PaymentTransaction
            FOREIGN KEY (PaymentTransactionId) REFERENCES dbo.PaymentTransaction (PayTranId),
        CONSTRAINT FK_AccSettlement_Store
            FOREIGN KEY (StoreId) REFERENCES dbo.Stores (Id),
        CONSTRAINT FK_AccSettlement_ChannelFA
            FOREIGN KEY (ChannelFinancialAccountId) REFERENCES dbo.FinancialAccounts (Id),
        CONSTRAINT FK_AccSettlement_ReceivingFA
            FOREIGN KEY (ReceivingFinancialAccountId) REFERENCES dbo.FinancialAccounts (Id)
    );
    CREATE UNIQUE INDEX UX_AccSettlement_PaymentTransaction ON dbo.AccSettlement (PaymentTransactionId);
    CREATE INDEX IX_AccSettlement_Store_Date ON dbo.AccSettlement (StoreId, TransactionDate);
    PRINT 'Table AccSettlement created.';
END
ELSE
    PRINT 'Table AccSettlement already exists - no change.';
GO

/* ---- 3. AccCashSwap (cash swap between branches, in transit) ----------- */
IF OBJECT_ID(N'dbo.AccCashSwap', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.AccCashSwap
    (
        SwapId       INT           NOT NULL IDENTITY(1,1) PRIMARY KEY,
        SwapNo       NVARCHAR(30)  NOT NULL UNIQUE,
        FromStoreId  INT           NOT NULL,
        ToStoreId    INT           NOT NULL,
        FromBranchCode NVARCHAR(20) NOT NULL,
        ToBranchCode   NVARCHAR(20) NOT NULL,
        Amount       DECIMAL(18,2) NOT NULL CHECK (Amount > 0),
        Remarks      NVARCHAR(500) NULL,
        Status       NVARCHAR(20)  NOT NULL DEFAULT 'IN_TRANSIT' CHECK (Status IN ('IN_TRANSIT','RECEIVED','CANCELLED')),
        SendEntryId  BIGINT        NULL,
        ReceiveEntryId BIGINT      NULL,
        SentBy       INT           NOT NULL,
        SentDate     DATETIME      NOT NULL DEFAULT GETDATE(),
        ReceivedBy   INT           NULL,
        ReceivedDate DATETIME      NULL,
        CancelledBy  INT           NULL,
        CancelledDate DATETIME     NULL,
        CONSTRAINT FK_AccCashSwap_FromStore FOREIGN KEY (FromStoreId) REFERENCES dbo.Stores (Id),
        CONSTRAINT FK_AccCashSwap_ToStore   FOREIGN KEY (ToStoreId)   REFERENCES dbo.Stores (Id),
        CONSTRAINT CK_AccCashSwap_Different CHECK (FromStoreId <> ToStoreId)
    );
    CREATE INDEX IX_AccCashSwap_Status_To ON dbo.AccCashSwap (Status, ToStoreId);
    CREATE INDEX IX_AccCashSwap_From     ON dbo.AccCashSwap (FromStoreId, Status);
    PRINT 'Table AccCashSwap created.';
END
ELSE
    PRINT 'Table AccCashSwap already exists - no change.';
GO

/* ---- 4. AccBranchLoan + repayments ------------------------------------ */
IF OBJECT_ID(N'dbo.AccBranchLoan', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.AccBranchLoan
    (
        LoanId           INT           NOT NULL IDENTITY(1,1) PRIMARY KEY,
        LoanNo           NVARCHAR(30)  NOT NULL UNIQUE,
        SenderStoreId    INT           NOT NULL,
        ReceiverStoreId  INT           NOT NULL,
        SenderBranchCode NVARCHAR(20)  NOT NULL,
        ReceiverBranchCode NVARCHAR(20) NOT NULL,
        Amount           DECIMAL(18,2) NOT NULL CHECK (Amount > 0),
        RemainingAmount  DECIMAL(18,2) NOT NULL CHECK (RemainingAmount >= 0),
        Status           NVARCHAR(20)  NOT NULL DEFAULT 'PENDING' CHECK (Status IN ('PENDING','ACCEPTED','CLOSED','CANCELLED')),
        Remarks          NVARCHAR(500) NULL,
        SentBy           INT           NOT NULL,
        SentDate         DATETIME      NOT NULL DEFAULT GETDATE(),
        AcceptedBy       INT           NULL,
        AcceptedDate     DATETIME      NULL,
        SendEntryId      BIGINT        NULL,
        AcceptEntryId    BIGINT        NULL,
        CONSTRAINT FK_AccBranchLoan_Sender   FOREIGN KEY (SenderStoreId)   REFERENCES dbo.Stores (Id),
        CONSTRAINT FK_AccBranchLoan_Receiver FOREIGN KEY (ReceiverStoreId) REFERENCES dbo.Stores (Id),
        CONSTRAINT CK_AccBranchLoan_Different CHECK (SenderStoreId <> ReceiverStoreId)
    );
    CREATE INDEX IX_AccBranchLoan_Status_Receiver ON dbo.AccBranchLoan (Status, ReceiverStoreId);
    CREATE INDEX IX_AccBranchLoan_Sender ON dbo.AccBranchLoan (SenderStoreId);
    PRINT 'Table AccBranchLoan created.';
END
ELSE
    PRINT 'Table AccBranchLoan already exists - no change.';
GO

IF OBJECT_ID(N'dbo.AccBranchLoanRepayment', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.AccBranchLoanRepayment
    (
        RepaymentId  INT           NOT NULL IDENTITY(1,1) PRIMARY KEY,
        LoanId       INT           NOT NULL,
        Amount       DECIMAL(18,2) NOT NULL CHECK (Amount > 0),
        EntryId      BIGINT        NULL,
        CreatedBy    INT           NOT NULL,
        CreatedDate  DATETIME      NOT NULL DEFAULT GETDATE(),
        CONSTRAINT FK_AccBranchLoanRepayment_Loan
            FOREIGN KEY (LoanId) REFERENCES dbo.AccBranchLoan (LoanId)
    );
    CREATE INDEX IX_AccBranchLoanRepayment_Loan ON dbo.AccBranchLoanRepayment (LoanId);
    PRINT 'Table AccBranchLoanRepayment created.';
END
ELSE
    PRINT 'Table AccBranchLoanRepayment already exists - no change.';
GO

/* ---- 5. Link PaymentTransaction -> AccSettlement (optional, nullable) -- */
IF COL_LENGTH(N'dbo.PaymentTransaction', N'SettlementId') IS NULL
BEGIN
    ALTER TABLE dbo.PaymentTransaction ADD SettlementId INT NULL;
    PRINT 'PaymentTransaction.SettlementId added.';
END
ELSE
    PRINT 'PaymentTransaction.SettlementId already exists - no change.';
GO

PRINT 'Treasury_Tables_DBChanges.sql complete.';
GO
