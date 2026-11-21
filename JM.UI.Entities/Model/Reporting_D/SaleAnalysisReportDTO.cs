using System;
using System.Collections.Generic;

namespace JM.UI.Entities.Model.Reporting_D
{
    public class SaleAnalysisBranchQtyDTO
    {
        public int StoreId { get; set; }
        public string StoreName { get; set; } = string.Empty;
        public decimal Qty { get; set; }
    }

    public class SaleAnalysisRowDTO
    {
        public int ItemId { get; set; }
        public string Barcode { get; set; } = string.Empty;
        public string ItemName { get; set; } = string.Empty;

        public int GroupId { get; set; }
        public string GroupName { get; set; } = string.Empty;
        public int SubGroupId { get; set; }
        public string SubGroupName { get; set; } = string.Empty;

        public string Origin { get; set; } = string.Empty;
        public string Features { get; set; } = string.Empty;
        public string Catalogue { get; set; } = string.Empty;
        public string Color { get; set; } = string.Empty;

        public decimal SalePrice { get; set; }

        public decimal QtyPurchased { get; set; }
        public decimal QtySold { get; set; }
        public decimal Revenue { get; set; }

        public decimal CurrentStock { get; set; }

        public string Classification { get; set; } = "Normal";

        public List<SaleAnalysisBranchQtyDTO> BranchQtys { get; set; } = new();
    }

    public class SaleAnalysisKpiDTO
    {
        public int TotalItems { get; set; }
        public decimal TotalQty { get; set; }
        public decimal TotalRevenue { get; set; }
        public int FastCount { get; set; }
        public int SlowCount { get; set; }
        public int NormalCount { get; set; }
        public int ZeroSalesCount { get; set; }
    }

    public class StoreLiteDTO
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
    }

    public class SaleAnalysisReportDTO
    {
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public int FastThreshold { get; set; }
        public int SlowThreshold { get; set; }

        public List<StoreLiteDTO> Stores { get; set; } = new();
        public List<SaleAnalysisRowDTO> Rows { get; set; } = new();
        public SaleAnalysisKpiDTO Kpi { get; set; } = new();
    }
}
