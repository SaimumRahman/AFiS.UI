using System;

namespace JM.UIWeb.CustomBase
{
    public static class AccountingReportOptions
    {
        /// <summary>Known acc_branch codes used by the accounting module.</summary>
        public static readonly string[] BranchCodes = { "HO", "BT", "GEC", "HLS", "NM", "FAC" };

        /// <summary>Known acc_party types used by the accounting module.</summary>
        public static readonly string[] PartyTypes = { "SUPPLIER", "CUSTOMER", "EMPLOYEE", "OWNER", "WORKER" };

        /// <summary>First day of the current fiscal year (BCS FY runs July - June).</summary>
        public static DateTime CurrentFiscalYearStart()
        {
            var now = DateTime.Today;
            return now.Month >= 7 ? new DateTime(now.Year, 7, 1) : new DateTime(now.Year - 1, 7, 1);
        }
    }
}