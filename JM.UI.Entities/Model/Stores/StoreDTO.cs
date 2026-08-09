using System;
using System.Collections.Generic;
using System.Text;

namespace JM.UI.Entities.Model.Stores
{
    public class StoreDTO
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Code { get; set; }
        public string? Address { get; set; }
        public string? Contact { get; set; }
        public string? Email { get; set; }
        public string? VAT { get; set; }
        public string? TIN { get; set; }
        public string? LetterHeadFile { get; set; }
        public bool UseLetterHead { get; set; }
        public DateTime CreatedOn { get; set; }
        public string CreatedBy { get; set; } = string.Empty;
        public DateTime? ModifiedOn { get; set; }
        public string? ModifiedBy { get; set; }
        public List<int> FinancialAccountIds { get; set; } = new();
        public List<StoreAccountDTO> StoreAccounts { get; set; } = new();
    }
}
