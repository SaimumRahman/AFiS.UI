using System.Collections.Generic;

namespace JM.UI.Entities.Model.Accounting_D
{
    public class ChartOfAccountDTO
    {
        public int AccountId { get; set; }
        public short CompanyId { get; set; }
        public int? ParentId { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Nature { get; set; } = string.Empty;
        public bool IsGroup { get; set; }
        public bool RequiresParty { get; set; }
        public bool RequiresBranch { get; set; }
        public bool IsCashBank { get; set; }
        public bool IsActive { get; set; }
        public List<string> PartyTypes { get; set; } = new();
        public string? ParentName { get; set; }
        public int HasChildren { get; set; }
    }
}
