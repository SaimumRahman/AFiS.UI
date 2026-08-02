using System;
using System.Collections.Generic;

namespace JM.UI.Entities.Model.SalesPOS
{
    public class SaleMasterDTO
    {
        public int SaleMasterId { get; set; }
        public string InvoiceNo { get; set; } = "";
        public int? CustomerId { get; set; }
        public string? CustomerName { get; set; }
        public string? CustomerPhone { get; set; }
        public string? CustomerAddress { get; set; }
        public DateTime SalesDate { get; set; }
        public decimal SubTotal { get; set; }
        public decimal? CampaignDiscount { get; set; }
        public decimal? MembershipDiscount { get; set; }
        public decimal? InvoiceDiscount { get; set; }
        public string? InvoiceDiscountType { get; set; } = "Percentage";
        public decimal? VatAmount { get; set; }
        public decimal? VatPercentage { get; set; } = 5m;
        public decimal? ExchangeAmount { get; set; }
        public decimal? RoundingAmount { get; set; }
        public decimal NetAmount { get; set; }
        public decimal? PaidAmount { get; set; }
        public decimal? DueAmount { get; set; }
        public string? PaymentStatus { get; set; } = "Due";
        public string? SalesType { get; set; } = "Sale";
        public string? Remarks { get; set; }
        public int? SalesPersonId { get; set; }
        public string? SalesPersonName { get; set; }
        public int? ShiftId { get; set; }
        public int? StoreId { get; set; }
        public string? StoreName { get; set; }
        public bool IsActive { get; set; } = true;
        public string? ReturnInvoiceNo { get; set; }
        public bool IsReturnExchange { get; set; }
        public int? MembershipTypeId { get; set; }
        public string? MembershipTypeName { get; set; }
        public int? DiscountRate { get; set; }
        public int? CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public int? LastModifiedBy { get; set; }
        public DateTime? LastModifiedDate { get; set; }

        // API-aligned fields
        public decimal TotalBill { get; set; }
        public decimal TotalDiscount { get; set; }
        public decimal TotalPaid { get; set; }
        public decimal TotalDue { get; set; }
        public decimal? TotalVat { get; set; }
        public bool? IsPaid { get; set; }
        public bool? IsBooking { get; set; }
        public bool? IsDraft { get; set; }
        public bool? IsDeleted { get; set; }

        public List<SaleDetailDTO> SaleDetails { get; set; } = new();
        public List<PaymentTransactionDTO> PaymentTransactions { get; set; } = new();
    }
}