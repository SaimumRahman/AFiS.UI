using System;

namespace JM.UI.Entities.Model.FinancialAccounts
{
    public class FinancialAccountDTO
    {
        public int Id { get; set; }
        public int FinancialAccountTypeId { get; set; }
        public string FinancialAccountTypeName { get; set; } = string.Empty;
        public int? MFSTypeId { get; set; }
        public string MFSTypeName { get; set; } = string.Empty;
        public int? BankId { get; set; }
        public string BankName { get; set; } = string.Empty;
        public string AccountNo { get; set; } = string.Empty;
        public string Remarks { get; set; } = string.Empty;
    }
}