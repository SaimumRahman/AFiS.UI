namespace JM.UI.Entities.Model.Customer
{
    public class CustomerModelDTO
    {
        public int CustomerID { get; set; }
        public string? CustomerName { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Email { get; set; }
        public string? Address { get; set; }
        public DateTime? CreatedDate { get; set; }
        public bool IsActive { get; set; } = true;
        public string? CustomerCode { get; set; }
        public string? CustomerType { get; set; }
        public decimal CurrentBalance { get; set; }
        public int? CreatedBy { get; set; }
    }
}