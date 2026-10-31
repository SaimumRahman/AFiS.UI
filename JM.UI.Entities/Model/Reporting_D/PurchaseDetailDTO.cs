using System;

namespace JM.UI.Entities.Model.Reporting_D
{
    public class PurchaseDetailDTO
    {
        public int PurchaseId { get; set; }
        public string ChallanNo { get; set; } = string.Empty;
        public string BillNo { get; set; } = string.Empty;
        public DateTime BillDate { get; set; }
        public string SupplierName { get; set; } = string.Empty;
        public string GroupName { get; set; } = string.Empty;
        public string ItemName { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public decimal ProductPrice { get; set; }
        public decimal CarryingCost { get; set; }
        public decimal OtherCost { get; set; }
        public decimal OperationalCost { get; set; }
        public decimal VatAmount { get; set; }
        public decimal CostPrice { get; set; }
        public decimal Quantity { get; set; }
        public string Uom { get; set; } = string.Empty;
        public decimal PurTotal { get; set; }
        public decimal SalePrice { get; set; }
        public decimal SaleTotal { get; set; }
    }
}
