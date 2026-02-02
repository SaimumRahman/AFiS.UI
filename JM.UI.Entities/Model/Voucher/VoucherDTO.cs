using System;
using System.Collections.Generic;
using System.Text;

namespace JM.UI.Entities.Model.Voucher
{
    public class VoucherDTO
    {
        public int Id { get; set; }
        public DateTime VoucherDate { get; set; }
        public int VoucherType { get; set; } // 1=Payment, 2=Receipt, 3=Journal, etc.
        public int StoreId { get; set; }
        public int CreatedBy { get; set; }
        public int VoucherNo { get; set; }
        public string? Description { get; set; }
        public string UserName { get; set; } = string.Empty;
        public List<VoucherDetailDTO> VoucherDetails { get; set; } = new();
    }
}
