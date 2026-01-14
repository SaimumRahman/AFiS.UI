using System;

namespace JM.UI.Entities.Model.Vouchers
{
    public class VoucherModelDTO
    {
        public int Id { get; set; }
        public DateTime VoucherDate { get; set; } = DateTime.Now;
        public int VoucherType { get; set; }
        public int StoreId { get; set; }
        public int CreatedBy { get; set; }
        public int VoucherNo { get; set; }
        public string? Description { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string? StoreName { get; set; }
    }
}
