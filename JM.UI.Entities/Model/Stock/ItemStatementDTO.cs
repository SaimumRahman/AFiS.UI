using System;
using System.Collections.Generic;

namespace JM.UI.Entities.Model.Stock
{
    /// <summary>
    /// Comprehensive item statement (Head Office view): item metadata plus a
    /// per-size matrix of branch-wise stock movements. Mirrors the API DTO.
    /// </summary>
    public class ItemStatementDTO
    {
        public string ProductName { get; set; } = string.Empty;
        public string BaseBarcode { get; set; } = string.Empty;
        public string GroupName { get; set; } = string.Empty;
        public string BrandName { get; set; } = string.Empty;
        public string SupplierName { get; set; } = string.Empty;
        public string UnitName { get; set; } = string.Empty;
        public string ColorName { get; set; } = string.Empty;
        public string OriginName { get; set; } = string.Empty;
        public string CatalogueName { get; set; } = string.Empty;
        public decimal PurchasePrice { get; set; }
        public decimal SalePrice { get; set; }
        public DateTime? PurchaseDate { get; set; }
        public string PurchasedBy { get; set; } = string.Empty;

        public List<ItemStatementSizeDTO> Sizes { get; set; } = new();
    }

    public class ItemStatementSizeDTO
    {
        public string Barcode { get; set; } = string.Empty;
        public string SizeName { get; set; } = string.Empty;
        public List<string> Branches { get; set; } = new();
        public Dictionary<string, decimal> OpeningByBranch { get; set; } = new();
        public Dictionary<string, decimal> ClosingByBranch { get; set; } = new();
        public decimal TotalOpening { get; set; }
        public decimal TotalClosing { get; set; }
        public List<ItemStatementTxnDTO> Transactions { get; set; } = new();
    }

    public class ItemStatementTxnDTO
    {
        public DateTime Date { get; set; }
        public string Description { get; set; } = string.Empty;
        public string ReferenceNo { get; set; } = string.Empty;
        public string TransactionKind { get; set; } = string.Empty;
        public int TransactionTypeID { get; set; }
        public Dictionary<string, decimal> BranchDelta { get; set; } = new();
        public decimal RunningTotal { get; set; }
    }
}
