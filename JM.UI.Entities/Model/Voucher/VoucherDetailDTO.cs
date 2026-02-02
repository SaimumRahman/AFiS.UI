using System;
using System.Collections.Generic;
using System.Text;

namespace JM.UI.Entities.Model.Voucher
{
    // =============================================
    public class VoucherDetailDTO
    {
        public int Id { get; set; }
        public int VoucherId { get; set; }
        public int AccountId { get; set; }
        public decimal Debit { get; set; }
        public decimal Credit { get; set; }
        public string? Description { get; set; }
    }
}
