using JM;
using JM.UI;
using JM.UI.Entities;
using JM.UI.Entities.Model;
using JM.UI.Entities.Model.Approval;
using JM.UI.Entities.Model.Company;
using System;
using System.Collections.Generic;
using System.Text;

namespace JM.UI.Entities.Model.Company
{
    public class CompanyDTO
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string Contact { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string VAT { get; set; } = string.Empty;
        public string TIN { get; set; } = string.Empty;
        public DateTime CreatedOn { get; set; }
        public string CreatedBy { get; set; } = string.Empty;
        public DateTime? ModifiedOn { get; set; }
        public string ModifiedBy { get; set; } = string.Empty;
    }
}
