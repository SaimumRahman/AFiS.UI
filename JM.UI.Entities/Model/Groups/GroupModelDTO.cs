namespace JM.UI.Entities.Model.Groups;

public class GroupModelDTO
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public decimal VAT { get; set; }
}
public class GroupCodeDTO
{
    public string? Code { get; set; }
}