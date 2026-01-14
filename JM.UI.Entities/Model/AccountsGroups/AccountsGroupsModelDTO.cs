namespace JM.UI.Entities.Model.AccountsGroups
{
    public class AccountsGroupsModelDTO
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int StoreId { get; set; }
        
        // UI Helper
        public string? StoreName { get; set; }
    }
}
