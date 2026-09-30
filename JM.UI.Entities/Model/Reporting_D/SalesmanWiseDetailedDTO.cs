using System;

namespace JM.UI.Entities.Model.Reporting_D
{
    public class SalesmanWiseDetailedDTO
    {
        public int SaleMasterId { get; set; }
        public string SalesmanName { get; set; } = string.Empty;
        public string InvoiceNo { get; set; } = string.Empty;
        public DateTime SaleDate { get; set; }
        public string Remarks { get; set; } = string.Empty;
        public string ItemCode { get; set; } = string.Empty;
        public string ItemName { get; set; } = string.Empty;
        public decimal Quantity { get; set; }
        public string Uom { get; set; } = string.Empty;
        public decimal Rate { get; set; }
        public decimal GrossAmount { get; set; }
        public decimal CampDisc { get; set; }
        public decimal InvDisc { get; set; }
        public decimal TotalAmt { get; set; }
        public decimal ExchangeAmt { get; set; }
        public decimal NetPaid { get; set; }
        public decimal Dues { get; set; }
        public int LineId { get; set; }
    }
}
