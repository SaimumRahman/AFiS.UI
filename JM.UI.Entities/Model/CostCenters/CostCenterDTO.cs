namespace JM.UI.Entities.Model.CostCenters
{
    public class CostCenterDTO
    {
        public int BranchId { get; set; }
        public int CompanyId { get; set; } = 1;
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? BranchKind { get; set; }
        public bool IsActive { get; set; } = true;
    }
}