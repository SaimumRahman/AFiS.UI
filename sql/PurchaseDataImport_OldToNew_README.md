# Purchase data import: `fiaznewlive` → new database

Script: `PurchaseDataImport_OldToNew.sql`

Copies the **purchase history** from the old database `fiaznewlive` (on a **different SQL Server**) into an **empty (cleared) copy of `AFIS_MAH_LIVE`**.

The source tables are first copied into a local staging schema (`imp`) through an `OPENDATASOURCE` connection string. The import itself then runs locally — **no linked server, no MSDTC, no distributed transaction required**.

Tested against a throwaway restore of `AFIS_MAH_LIVE` + `FreshStart_DBCleanup.sql`:

| Data | Old rows | Imported rows |
|---|---|---|
| Purchases | 210 | 210 |
| Purchase items | 1,643 | 1,643 |
| Purchase total | 3,173,838.00 | 3,173,838.00 |
| Suppliers | 57 | 57 |
| Items | 1,038 | 1,038 |
| Supplier payments | 77 | 77 |
| Payment total | 1,844,086.00 | 1,844,086.00 |
| Batch numbers | 28 | 28 |

Both the default run and a run with every optional block enabled completed with zero orphan rows and reconciled totals.

---

## 1. Create the target database

**Do not use a truly schema-only backup.** The application needs its seed data (users, roles, routes, groups, stores, chart of accounts, fiscal periods). A schema-only database has none of that, so the script stops with a clear message.

Recommended:

1. Take a **full backup** of `AFIS_MAH_LIVE`.
2. Restore it as the new database, e.g. `AFIS_MAH_NEWUSER`.
3. Run `sql\FreshStart_DBCleanup.sql` in the new database.
   * This keeps master/seed data and clears every transaction.
   * If you run it from `sqlcmd`, use `sqlcmd -I` (it needs `QUOTED_IDENTIFIER ON`; SSMS already has it on).
4. Take a backup of the cleaned database before importing.

The new database can be on **any SQL Server instance** — the old database is read through a raw connection string embedded in the script.

---

## 2. Configure the script

Open `PurchaseDataImport_OldToNew.sql` and change:

```sql
USE [AFIS_MAH_LIVE_NEW];   -- <<< your new database name
```

Change the **source connection** to match the old server:

```sql
DECLARE @SrcServer   nvarchar(200) = N'202.4.117.56,1435';  -- server,port
DECLARE @SrcDb       sysname       = N'fiaznewlive';        -- old DB name
DECLARE @SrcUser     nvarchar(100) = N'sa';                  -- login
DECLARE @SrcPassword nvarchar(200) = N'Sobuj@4321';          -- password
DECLARE @SrcProvider nvarchar(50)  = N'MSOLEDBSQL19';        -- OLE DB driver on the NEW server
```

The driver `MSOLEDBSQL19` is the Microsoft OLE DB Driver 19 for SQL Server (installed with SQL Server 2025). If your target server only has the older driver, use `N'MSOLEDBSQL'`.

Optional switches (defaults shown):

```sql
DECLARE @IncludeDrafts     bit = 0;  -- 72 drafts / 933 lines
DECLARE @IncludeStock      bit = 0;  -- stock snapshot + purchase stock movements
DECLARE @IncludeAccounting bit = 0;  -- 46 posted PUR/REVERSAL journals
```

What each block does:

* **Drafts** – copies `PurchaseDrafts` / `PurchaseDraftItems` exactly as they are (most of them are old inactive drafts).
* **Stock** – copies `StockMaster` as a snapshot and only purchase / opening `StockDetail` rows (`TransactionTypeID` 1 and 3). The old stock snapshot already includes the effect of old sales, so current stock quantities match the old system.
* **Accounting** – copies the old purchase journals (39 `PUR` + 7 `REVERSAL`) with entry/line ids preserved, accounts remapped by account code, and parties remapped through the imported suppliers. Journal guard triggers are suspended during the copy and re-enabled immediately; the script also verifies every entry is balanced and that required parties are present. Voucher numbering is advanced so new vouchers do not collide.

The three columns that are new in the target `SupplierPayments` (`FinancialAccountId`, `PaymentLedgerCode`, `TransactionNo`) are written as `NULL`. Old payments were cash/bank payments recorded outside the new GL, so no ledger entries are created for them.

---

## 3. Run it

In SSMS: open the script, pick the **new** database, execute.

From `sqlcmd`:

```bat
sqlcmd -S <NEW_SERVER>,<PORT> -U sa -P "..." -C -I -d AFIS_MAH_NEWUSER ^
       -b -i "PurchaseDataImport_OldToNew.sql"
```

The whole import is **one transaction**. Any error rolls everything back and the script prints the error number, message and line. The target is also checked before anything is changed:

* refuses to run in the old database
* refuses a target that already has Items / Suppliers / Purchases / lines / payments (and drafts/stock/journals when those blocks are enabled)
* refuses a target with no application seed
* verifies the old data has no dangling item/supplier/store references and no duplicate `SystemInvoiceNo`

---

## 4. What gets remapped (important)

The old and new databases are **different businesses with different master data**, so ids are *not* shared:

* **Preserved ids:** `Items`, `Suppliers`, `Purchases`, `PurchaseItems`, `SupplierPayments`, `PurchaseDrafts`, stock ids, journal entry/line ids. Parent/child links stay intact.
* **Remapped ids:** Colors, Sizes, Measurement Units, Brands, Origins, Item Catalogue, Item Features, Groups, SubGroups, Designs, Banks.
  * Matching is by normalised name (case/space insensitive).
  * If the name exists in the target, the old row maps to it.
  * Otherwise the old row is **added** to the target with a new id; the target's own master data is never deleted.
* Old rows with `0` or dangling lookup references are mapped to `NULL` where the column is nullable, or to fallback rows where it is `NOT NULL`:
  * group `IMPORTED (OLD DB)`, code `IMP-000`
  * sub-group `UNMAPPED`, code `IMP-UNM`
* The old `ItemOrigin` table contains duplicate `OriginId` values. The script collapses them to one name per id, preferring a name that already exists in the target.

`ProductSerialSequence` is moved ahead of the old value (1601), and every identity column is reseeded to its maximum imported value, so the application continues numbering correctly.

`SystemInvoiceNo` 0000214 is the last imported number; the next allocated number will be 0000215.

---

## 5. After the import

Check the report at the end of the script:

* old/new row counts per entity
* old/new purchase totals and payment totals
* next system invoice number
* number of items that landed in the fallback group

Useful spot checks:

```sql
-- supplier outstanding should show real due amounts
SELECT * FROM vw_SupplierOutstanding;
SELECT * FROM vw_SupplierLedger;

-- purchases and lines
SELECT COUNT(*) FROM Purchases;
SELECT COUNT(*) FROM PurchaseItems;

-- no orphan references
SELECT COUNT(*) FROM PurchaseItems pi
WHERE NOT EXISTS (SELECT 1 FROM Purchases p WHERE p.Id = pi.PurchaseId);
```

Then start the API / UI against the new database and open Purchases, Suppliers and the supplier ledger.

---

## 6. Caveats

* Old supplier payments are imported as records only; their GL vouchers are not created (they did not exist in a compatible form). Supplier dues are still correct because they are calculated from purchases minus payments.
* If `@IncludeStock = 0`, `StockMaster` / `StockDetail` stay empty in the target until you import stock or post opening stock.
* If `@IncludeAccounting = 0`, the old purchase journals are not imported; the new chart of accounts starts with no purchase vouchers.
* Old drafts are old/inactive; enabling them will show 72 drafts in the UI.
* The script is intentionally not re-runnable against a populated target. To try again, restore the pre-import backup (or re-run `FreshStart_DBCleanup.sql`).