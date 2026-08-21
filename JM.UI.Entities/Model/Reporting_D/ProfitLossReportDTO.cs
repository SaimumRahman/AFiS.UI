using System;
using System.Collections.Generic;

namespace JM.UI.Entities.Model.Reporting_D
{
    public class ProfitLossReportDTO
    {
        public int ItemId { get; set; }
        public string? Barcode { get; set; }
        public string? ItemName { get; set; }

        /// <summary>Total quantity received into stock.</summary>
        public decimal TotalIn { get; set; }

        /// <summary>Total quantity issued out of stock.</summary>
        public decimal TotalOut { get; set; }

        /// <summary>Weighted average purchase (cost) price.</summary>
        public decimal AvgPurchasePrice { get; set; }

        /// <summary>Total quantity sold.</summary>
        public decimal TotalSaleQty { get; set; }

        /// <summary>Total sales amount.</summary>
        public decimal TotalSaleAmount { get; set; }

        /// <summary>Current on-hand stock = TotalIn - TotalOut.</summary>
        public decimal CurrentStock { get; set; }

        /// <summary>Estimated profit = TotalSaleAmount - (AvgPurchasePrice * TotalSaleQty).</summary>
        public decimal Profit => TotalSaleAmount - (AvgPurchasePrice * TotalSaleQty);
    }
}
