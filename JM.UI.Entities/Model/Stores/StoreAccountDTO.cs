using System;

namespace JM.UI.Entities.Model.Stores
{
    public class StoreAccountDTO
    {
        public int Id { get; set; }
        public int StoreId { get; set; }
        public int FinancialAccountId { get; set; }
        public string AccountNo { get; set; } = string.Empty;
        public string AccountName { get; set; } = string.Empty;
        public string AccountType { get; set; } = string.Empty;
        public string Remarks { get; set; } = string.Empty;
    }
}
