using System;
using System.Collections.Generic;
using System.Text;

namespace JM.UI.Entities.Model.Employees
{
    public class EmployeeModelDTO
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string EmployeeCode { get; set; } = string.Empty;
        public DateTime? DateOfBirth { get; set; }
        public string Gender { get; set; } = string.Empty;
        public string BloodGroup { get; set; } = string.Empty;
        public string Religion { get; set; } = string.Empty;
        public string FatherName { get; set; } = string.Empty;
        public string MotherName { get; set; } = string.Empty;
        public string MaritalStatus { get; set; } = string.Empty;
        public string SpouseName { get; set; } = string.Empty;
        public string PresentAddress { get; set; } = string.Empty;
        public string PermanentAddress { get; set; } = string.Empty;
        public string Contact { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string EmergencyContact { get; set; } = string.Empty;
        public string EmergencyContactPerson { get; set; } = string.Empty;
        public string EmergencyContactPersonAddress { get; set; } = string.Empty;
        public string Relation { get; set; } = string.Empty;
        public int? BankId { get; set; }
        public string BankAccountNumber { get; set; } = string.Empty;
        public string Picture { get; set; } = string.Empty;
        public DateTime? DateJoined { get; set; }
        public DateTime? DateReleased { get; set; }
        public string NID { get; set; } = string.Empty;
        public string ReferredBy { get; set; } = string.Empty;
        public int? AccountId { get; set; }
        public int? StoreId { get; set; }
        public int? DesignationId { get; set; }
        public decimal? BasicSalary { get; set; }
        public int? DutyType { get; set; } = 1;
        public int? ShiftId { get; set; }
        public int Status { get; set; }
        public string CreatedBy { get; set; } = string.Empty;
        public DateTime CreatedOn { get; set; }
        public string ModifiedBy { get; set; } = string.Empty;
        public DateTime? ModifiedOn { get; set; }

        // Navigation Properties
        public string BankName { get; set; } = string.Empty;
        public string StoreName { get; set; } = string.Empty;
        public string DesignationName { get; set; } = string.Empty;
        public string ShiftName { get; set; } = string.Empty;
    }
}
