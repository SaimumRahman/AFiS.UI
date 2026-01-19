namespace JM.UI.Entities.Model.Items
{
    public class ItemModelDTO
    {
        public int Id { get; set; }
        public int SubGroupId { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal AlarmLevel { get; set; }
        public string? Barcode { get; set; }
        public int? SupplierId { get; set; }
        public byte[]? Image { get; set; }
        public decimal SalePrice { get; set; }
        public decimal WholeSalePrice { get; set; }
        public int MesurementUnitId { get; set; }
        public bool CountStockByColor { get; set; }
        public bool CountStockBySize { get; set; }
        public bool RawMaterial { get; set; }
        public bool FinishedGood { get; set; }
        public decimal LastCostPrice { get; set; }

        public string? SubGroupName { get; set; }
        public string? MesurementUnitName { get; set; }
        public string? SupplierName { get; set; }
    }
}
