using System;

namespace JM.UI.Entities.Model.Suppliers
{
    public class SupplierModelDTO
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int AccountId { get; set; }
        public string? ShortCode { get; set; }
        public string? Address { get; set; }
        public string? ContactNo { get; set; }
        public int SupplierType { get; set; }
        public bool BlockDue { get; set; }

        // UI Helpers
        public string? AccountName { get; set; }
    }
}
