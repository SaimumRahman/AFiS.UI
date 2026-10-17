/* =========================================================================
   GroupsRole_MaxDiscount_DBChanges.sql

   Adds GroupsRole.MaxDiscount: the highest invoice discount (%) a user of
   that group may give in Sales POS (Billing Summary -> Invoice Discount).

   Consumed by:
     - Group Roles page  (/GroupRoleAdd, /GroupRoleList)
     - Sales POS         (blocks finalise when the entered discount exceeds it)
     - InsertUpdateSaleHandler (authoritative server-side check)

   NULL / blank = no limit.   Idempotent / safe to re-run.   Target: AFIS_MAH_LIVE
   ========================================================================= */
SET NOCOUNT ON;
SET QUOTED_IDENTIFIER ON;

IF COL_LENGTH('dbo.GroupsRole', 'MaxDiscount') IS NULL
BEGIN
    ALTER TABLE dbo.GroupsRole ADD MaxDiscount DECIMAL(18,2) NULL;
    PRINT 'GroupsRole.MaxDiscount added.';
END
ELSE
BEGIN
    PRINT 'GroupsRole.MaxDiscount already exists - no change.';
END;
GO

/* ---- Verify ------------------------------------------------------------ */
SELECT 'AFTER' AS Phase,
       GroupId,
       GroupName,
       IsSystem,
       MaxDiscount
FROM dbo.GroupsRole
ORDER BY GroupId;
