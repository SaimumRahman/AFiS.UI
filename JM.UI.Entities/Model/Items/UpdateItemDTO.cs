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
    }
}
