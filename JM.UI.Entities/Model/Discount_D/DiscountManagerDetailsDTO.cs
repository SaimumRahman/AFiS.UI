using System;
using System.Collections.Generic;
using System.Text;

namespace JM.UI.Entities.Model.Discount_D
{
    public class DiscountManagerDetailsDTO
    {
        public int Id { get; set; }
        public int DiscountManagerId { get; set; }
        public int ItemId { get; set; }
        public string? ItemName { get; set; }
        public int? StoreId { get; set; }
        public string? StoreName { get; set; }
        public string? Barcode { get; set; }
        public decimal DiscountValue { get; set; }
        public int DiscountTypeId { get; set; }
        public string? DiscountTypeName { get; set; }
        public decimal CurrentSalePrice { get; set; }
        public string CreatedBy { get; set; } = string.Empty;
        public DateTime CreatedDate { get; set; }
        public string? ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }
    }
}
