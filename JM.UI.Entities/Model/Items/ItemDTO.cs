namespace JM.UI.Entities.Model.Items
{
    public class ItemDTO
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int? GroupId { get; set; }
        public string? GroupName { get; set; }
        public string? Barcode { get; set; }
        public int? SubGroupId { get; set; }
        public int? MesurementUnitId { get; set; }
        public string? SubGroupName { get; set; }
        public int? UnitId { get; set; }
        public string? UnitName { get; set; }
        public string? ShadeNo { get; set; }
        public string? MaterialType { get; set; }
        public string? Origin { get; set; }
        public string? Features { get; set; }
        public string? BrandColor { get; set; }
        public decimal? ProductPricePercentage { get; set; }
        public decimal? PurchasePrice { get; set; }
        public decimal? SalePrice { get; set; }
        public decimal? ReorderLevel { get; set; }
        public decimal? WholeSalePrice { get; set; }
        public decimal? LastCostPrice { get; set; }
        public int? AlarmLevel { get; set; }
        public bool IsActive { get; set; }
        public int? CreatedBy { get; set; }
        public DateTime? CreatedDate { get; set; }
        public int? LastModifiedBy { get; set; }
        public DateTime? LastModifiedDate { get; set; }
    }
}
