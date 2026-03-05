namespace JM.UI.Entities.Model.Designs
{
    public class DesignModelDTO
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public int SubGroupId { get; set; }
        public string? SubGroupName { get; set; }
    }
}
