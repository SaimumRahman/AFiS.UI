using System;

namespace JM.UI.Entities.Model.FinancialAccounts
{
    public class FinancialAccountDropdownDTO
    {
        public int Id { get; set; }
        public int FinancialAccountTypeId { get; set; }
        public string AccountNo { get; set; } = string.Empty;
        public string Remarks { get; set; } = string.Empty;
        public string AccountName { get; set; } = string.Empty;
        public string AccountType { get; set; } = string.Empty;
    }
}
