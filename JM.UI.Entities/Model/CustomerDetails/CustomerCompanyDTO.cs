using System;

namespace JM.UI.Entities.Model.CustomerDetails
{
    public class CustomerCompanyDTO
    {
        public int Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? Address { get; set; }
        public string? SpecialNote { get; set; }
        public int? DefaultCategoryId { get; set; }
        public bool IsActive { get; set; } = true;
    }
}
