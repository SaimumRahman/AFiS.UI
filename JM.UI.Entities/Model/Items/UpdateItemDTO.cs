using System;

namespace JM.UI.Entities.Model.Items
{
    public class UpdateItemDTO
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal? SalePrice { get; set; }
        public int? MesurementUnitId { get; set; }
        public int? AlarmLevel { get; set; }
        public int? LastModifiedBy { get; set; }
        public DateTime? LastModifiedDate { get; set; }

        // Brand / Origin / Features
        public int? BrandId { get; set; }
        public string? BrandName { get; set; }
        public int? OriginId { get; set; }
        public string? Origin { get; set; }
        public string? Features { get; set; }
        public int? ItemWiseFeatureId { get; set; }
    }
}
