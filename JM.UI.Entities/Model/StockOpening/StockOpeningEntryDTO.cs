using System;
using System.Collections.Generic;

namespace JM.UI.Entities.Model.StockOpening
{
    public class StockOpeningEntryDTO
    {
        public int ReferenceNo { get; set; }
        public int StoreId { get; set; }
        public string? StoreName { get; set; }
        public DateTime TransectionDate { get; set; }
        public string? Remarks { get; set; }
        public int CreatedBy { get; set; }
        
        public string? ItemName { get; set; }
        public string? Barcode { get; set; }
        
        public decimal TotalQuantity { get; set; }
        public decimal TotalAmount { get; set; }
        
        public List<StockOpeningItemDTO> Items { get; set; } = new List<StockOpeningItemDTO>();
    }
}
