using System;

namespace JM.UI.Entities.Model.VoucherDetails
{
    public class VoucherDetailsModelDTO
    {
        public int Id { get; set; }
        public int VoucherId { get; set; }
        public int AccountId { get; set; }
        public decimal Debit { get; set; }
        public decimal Credit { get; set; }
        public string? Description { get; set; }
        // for dropdowns
        public int? VoucherNo { get; set; }
        public string? AccountName { get; set; }
    }
}
