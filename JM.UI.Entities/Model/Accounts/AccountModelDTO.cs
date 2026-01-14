using System;

namespace JM.UI.Entities.Model.Accounts
{
    public class AccountModelDTO
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int AccountsGroupId { get; set; }
        public int? ParentId { get; set; }
        public int LevelNo { get; set; }
        public decimal OpeningBalanceDebit { get; set; }
        public decimal OpeningBalanceCredit { get; set; }
        public bool IsTradingAccount { get; set; }
        public bool IsReceivePaymentAccount { get; set; }
        public bool IsManufacturingAccount { get; set; }
        public int AccountType { get; set; }
        public bool IsActive { get; set; } = true;
        public string CreatedBy { get; set; } = "Admin"; // Default for now
        public DateTime CreatedOn { get; set; } = DateTime.Now;
        public string? ModifiedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }

        // UI Helpers
        public string? AccountsGroupName { get; set; }
        public string? ParentName { get; set; }
    }
}
