using System;
using System.Collections.Generic;
using System.Text;

namespace JM.UI.Entities.Model.Stock
{
    public class StockTransactionDTO
    {
        public int Id { get; set; }
        public int TransectionType { get; set; } // 1=Purchase, 2=Sale, 3=Return, etc.
        public DateTime TransectionDate { get; set; }
        public decimal Quantity { get; set; }
        public decimal TradePrice { get; set; }
        public decimal MRP { get; set; }
        public int ReferenceNo { get; set; }
        public int UserId { get; set; }
        public string? UserName { get; set; }
        public int? ColorId { get; set; }
        public int? SizeId { get; set; }
        public int ItemId { get; set; }
        public decimal PrevQuantity { get; set; }
        public decimal BalanceQuantity { get; set; }
        public decimal AverageTradePrice { get; set; }
        public int? StoreId { get; set; }
    }
}
