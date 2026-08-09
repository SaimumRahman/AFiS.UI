-- =============================================
-- Financial Accounts - Dapper Queries for API Endpoints
-- =============================================

-- Table: FinancialAccountTypes
CREATE TABLE FinancialAccountTypes (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Name NVARCHAR(100) NOT NULL
);

-- Table: MFSTypes
CREATE TABLE MFSTypes (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Name NVARCHAR(100) NOT NULL
);

-- Table: FinancialAccounts
CREATE TABLE FinancialAccounts (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    FinancialAccountTypeId INT NOT NULL,
    MFSTypeId INT NULL,
    BankId INT NULL,
    AccountNo NVARCHAR(100) NOT NULL,
    Remarks NVARCHAR(500) NULL,
    CONSTRAINT FK_FinancialAccounts_FinancialAccountType FOREIGN KEY (FinancialAccountTypeId) REFERENCES FinancialAccountTypes(Id),
    CONSTRAINT FK_FinancialAccounts_MFSType FOREIGN KEY (MFSTypeId) REFERENCES MFSTypes(Id),
    CONSTRAINT FK_FinancialAccounts_Bank FOREIGN KEY (BankId) REFERENCES Banks(Id)
);

-- Seed: Financial Account Types
INSERT INTO FinancialAccountTypes (Name) VALUES ('Bank Account');
INSERT INTO FinancialAccountTypes (Name) VALUES ('Current Account');
INSERT INTO FinancialAccountTypes (Name) VALUES ('Savings Account');
INSERT INTO FinancialAccountTypes (Name) VALUES ('Cash Account');
INSERT INTO FinancialAccountTypes (Name) VALUES ('MFS Account');
INSERT INTO FinancialAccountTypes (Name) VALUES ('Fixed Deposit');
INSERT INTO FinancialAccountTypes (Name) VALUES ('Loan Account');

-- Seed: MFS Types
INSERT INTO MFSTypes (Name) VALUES ('bKash');
INSERT INTO MFSTypes (Name) VALUES ('Nagad');
INSERT INTO MFSTypes (Name) VALUES ('Rocket');
INSERT INTO MFSTypes (Name) VALUES ('Upay');
INSERT INTO MFSTypes (Name) VALUES ('Rupay');
