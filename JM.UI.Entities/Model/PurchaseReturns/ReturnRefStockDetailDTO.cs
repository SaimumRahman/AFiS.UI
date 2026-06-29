namespace JM.UI.Entities.Model.PurchaseReturns
{
    public class ReturnRefStockDetailDTO
    {
        public string ProductName { get; set; } = string.Empty;
        public string? Barcode { get; set; }
        public decimal? CurrentStock { get; set; }
        public decimal? PurchasePrice { get; set; }
        public int? SupplierId { get; set; }
        public string? SupplierName { get; set; }
        public string? InvoiceNo { get; set; }
        public int? MesurementUnitId { get; set; }
        public string? UOMName { get; set; }

        // Workflow fields (not from API — used for the return grid)
        public decimal Quantity { get; set; }
        public decimal TradePrice { get; set; }
    }
}
