using System.Collections.Generic;

namespace JM.UI.Entities.Model.Dashboard
{
    public class DashboardTrendPointDTO
    {
        public string Label { get; set; } = string.Empty;
        public decimal Amount { get; set; }
    }

    public class DashboardPaymentMixDTO
    {
        public decimal Cash { get; set; }
        public decimal Mfs { get; set; }
        public decimal Bank { get; set; }
        public decimal Total { get; set; }
    }

    public class DashboardBranchRowDTO
    {
        public int StoreId { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal Net { get; set; }
        public decimal Discount { get; set; }
        public decimal Expenses { get; set; }
        public decimal ClosingCash { get; set; }
        public string Status { get; set; } = string.Empty;
    }

    public class DashboardPerformerDTO
    {
        public int Rank { get; set; }
        public string Who { get; set; } = string.Empty;
        public int Inv { get; set; }
        public decimal Amt { get; set; }
    }

    public class DashboardLowStockDTO
    {
        public string Item { get; set; } = string.Empty;
        public string Branch { get; set; } = string.Empty;
        public decimal Left { get; set; }
    }

    public class AdminDashboardDTO
    {
        public string PeriodLabel { get; set; } = string.Empty;

        public decimal NetSales { get; set; }
        public decimal Discount { get; set; }
        public int Invoices { get; set; }
        public int Bookings { get; set; }
        public decimal Expenses { get; set; }
        public decimal ClosingCash { get; set; }
        public int ApprovalsClosing { get; set; }
        public int ApprovalsAudit { get; set; }
        public int ApprovalsDiscount { get; set; }

        public List<DashboardTrendPointDTO> Trend { get; set; } = new();
        public DashboardPaymentMixDTO PaymentMix { get; set; } = new();
        public List<DashboardBranchRowDTO> Branches { get; set; } = new();
        public string TopSellingBranch { get; set; } = string.Empty;
        public string TopExpenseBranch { get; set; } = string.Empty;
        public List<DashboardPerformerDTO> Salesmen { get; set; } = new();
        public List<DashboardPerformerDTO> Cashiers { get; set; } = new();
        public List<DashboardLowStockDTO> LowStock { get; set; } = new();
    }
}
