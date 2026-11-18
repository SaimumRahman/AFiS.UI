using System;
using System.Collections.Generic;

namespace JM.UI.Entities.Model.Stock;

public class StockAgingStoreDTO
{
    public int StoreId { get; set; }
    public string StoreName { get; set; } = string.Empty;
    public string StoreCode { get; set; } = string.Empty;
    public bool IsHeadOffice { get; set; }
}

public class StockAgingCellDTO
{
    public int StoreId { get; set; }
    public decimal Qty { get; set; }
    public decimal Sold30 { get; set; }
    public string Status { get; set; } = "Healthy";
}

public class StockAgingRowDTO
{
    public int ItemId { get; set; }
    public string Barcode { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public string GroupName { get; set; } = string.Empty;
    public string BrandName { get; set; } = string.Empty;
    /// <summary>Item measurement unit exactly as saved on the item (e.g. PCS, MTR, YDS).</summary>
    public string UomName { get; set; } = "pcs";
    public decimal Cost { get; set; }
    public decimal Mrp { get; set; }
    public decimal TotalQty { get; set; }
    public decimal TotalCost { get; set; }
    public int AgeDays { get; set; }
    public DateTime? LastSaleDate { get; set; }
    public string Category { get; set; } = "Healthy";
    public string ActionText { get; set; } = string.Empty;
    public string ActionFrom { get; set; } = string.Empty;
    public string ActionTo { get; set; } = string.Empty;
    public decimal ActionQty { get; set; }
    public List<StockAgingCellDTO> Cells { get; set; } = new();
}

public class StockAgingKpiDTO
{
    public int TransferSkuCount { get; set; }
    public decimal TransferPcs { get; set; }
    public decimal TransferMrp { get; set; }
    public int StockoutCount { get; set; }
    public string StockoutByBranch { get; set; } = string.Empty;
    public decimal DeadCost { get; set; }
    public decimal DeadPcs { get; set; }
    public int OrphanDesignCount { get; set; }
}

public class StockAgingReportDTO
{
    public List<StockAgingStoreDTO> Stores { get; set; } = new();
    public List<StockAgingRowDTO> Rows { get; set; } = new();
    public StockAgingKpiDTO Kpi { get; set; } = new();
}