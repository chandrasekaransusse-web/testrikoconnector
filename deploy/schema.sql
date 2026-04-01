IF DB_ID('TallyConnector') IS NULL
    CREATE DATABASE TallyConnector;
GO
USE TallyConnector;
GO

CREATE TABLE dbo.ledger_groups (
    group_id nvarchar(64) NOT NULL PRIMARY KEY,
    name nvarchar(255) NOT NULL,
    parent_group_name nvarchar(255) NULL,
    is_reserved bit NOT NULL CONSTRAINT DF_ledger_groups_is_reserved DEFAULT (0),
    created_at_utc datetime2 NOT NULL CONSTRAINT DF_ledger_groups_created DEFAULT SYSUTCDATETIME(),
    updated_at_utc datetime2 NOT NULL CONSTRAINT DF_ledger_groups_updated DEFAULT SYSUTCDATETIME()
);

CREATE TABLE dbo.ledgers (
    ledger_id nvarchar(64) NOT NULL PRIMARY KEY,
    name nvarchar(255) NOT NULL,
    parent_group_name nvarchar(255) NULL,
    gst_in nvarchar(32) NULL,
    pan nvarchar(32) NULL,
    address_line nvarchar(500) NULL,
    is_bill_wise_on bit NOT NULL CONSTRAINT DF_ledgers_billwise DEFAULT (0),
    created_at_utc datetime2 NOT NULL CONSTRAINT DF_ledgers_created DEFAULT SYSUTCDATETIME(),
    updated_at_utc datetime2 NOT NULL CONSTRAINT DF_ledgers_updated DEFAULT SYSUTCDATETIME()
);

CREATE TABLE dbo.stock_groups (
    stock_group_id nvarchar(64) NOT NULL PRIMARY KEY,
    name nvarchar(255) NOT NULL,
    parent_stock_group_name nvarchar(255) NULL,
    created_at_utc datetime2 NOT NULL CONSTRAINT DF_stock_groups_created DEFAULT SYSUTCDATETIME(),
    updated_at_utc datetime2 NOT NULL CONSTRAINT DF_stock_groups_updated DEFAULT SYSUTCDATETIME()
);

CREATE TABLE dbo.stock_items (
    stock_item_id nvarchar(64) NOT NULL PRIMARY KEY,
    name nvarchar(255) NOT NULL,
    stock_group_name nvarchar(255) NULL,
    base_unit nvarchar(64) NULL,
    opening_balance decimal(18,4) NOT NULL CONSTRAINT DF_stock_items_opening_balance DEFAULT (0),
    opening_rate decimal(18,4) NOT NULL CONSTRAINT DF_stock_items_opening_rate DEFAULT (0),
    created_at_utc datetime2 NOT NULL CONSTRAINT DF_stock_items_created DEFAULT SYSUTCDATETIME(),
    updated_at_utc datetime2 NOT NULL CONSTRAINT DF_stock_items_updated DEFAULT SYSUTCDATETIME()
);

CREATE TABLE dbo.cost_centers (
    cost_center_id nvarchar(64) NOT NULL PRIMARY KEY,
    name nvarchar(255) NOT NULL,
    parent_cost_center_name nvarchar(255) NULL,
    created_at_utc datetime2 NOT NULL CONSTRAINT DF_cost_centers_created DEFAULT SYSUTCDATETIME(),
    updated_at_utc datetime2 NOT NULL CONSTRAINT DF_cost_centers_updated DEFAULT SYSUTCDATETIME()
);

CREATE TABLE dbo.vouchers (
    voucher_id nvarchar(64) NOT NULL PRIMARY KEY,
    voucher_number nvarchar(100) NOT NULL,
    voucher_date date NOT NULL,
    voucher_type nvarchar(100) NOT NULL,
    party_ledger_name nvarchar(255) NULL,
    total_amount decimal(18,2) NOT NULL,
    source_system nvarchar(50) NOT NULL,
    created_at_utc datetime2 NOT NULL CONSTRAINT DF_vouchers_created DEFAULT SYSUTCDATETIME(),
    updated_at_utc datetime2 NOT NULL CONSTRAINT DF_vouchers_updated DEFAULT SYSUTCDATETIME()
);

CREATE TABLE dbo.voucher_entries (
    voucher_id nvarchar(64) NOT NULL,
    line_no int NOT NULL,
    ledger_name nvarchar(255) NOT NULL,
    amount decimal(18,2) NOT NULL,
    is_debit bit NOT NULL,
    cost_center nvarchar(255) NULL,
    narration nvarchar(max) NULL,
    created_at_utc datetime2 NOT NULL CONSTRAINT DF_voucher_entries_created DEFAULT SYSUTCDATETIME(),
    updated_at_utc datetime2 NOT NULL CONSTRAINT DF_voucher_entries_updated DEFAULT SYSUTCDATETIME(),
    CONSTRAINT PK_voucher_entries PRIMARY KEY (voucher_id, line_no),
    CONSTRAINT FK_voucher_entries_vouchers FOREIGN KEY (voucher_id) REFERENCES dbo.vouchers(voucher_id)
);

CREATE INDEX IX_ledgers_parent_group_name ON dbo.ledgers(parent_group_name);
CREATE INDEX IX_stock_items_stock_group_name ON dbo.stock_items(stock_group_name);
CREATE INDEX IX_vouchers_voucher_date ON dbo.vouchers(voucher_date);
CREATE INDEX IX_voucher_entries_ledger_name ON dbo.voucher_entries(ledger_name);
