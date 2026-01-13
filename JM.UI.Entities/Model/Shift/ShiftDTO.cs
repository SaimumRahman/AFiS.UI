using JM;
using JM.UI;
using JM.UI.Entities;
using JM.UI.Entities.Model;
using JM.UI.Entities.Model.Approval;
using JM.UI.Entities.Model.Company;
using System;
using System.Collections.Generic;
using System.Text;

namespace JM.UI.Entities.Model.Shift;


public class ShiftDTO
{
    public int Id { get; set; }
    public int? DutyType { get; set; }
    public string? Name { get; set; }
    public string? ShiftCode { get; set; }
    public DateTime? Start { get; set; }  // Changed to nullable
    public DateTime? End { get; set; }    // Changed to nullable
    public DateTime? LateCountFrom { get; set; }
    public DateTime? CheckStart { get; set; }
    public DateTime? CheckEnd { get; set; }
    public string? LastLoginTime { get; set; }
    public decimal? TotalHours { get; set; }
    public DateTime? CheckStartFinish { get; set; }
    public DateTime? CheckEndFinish { get; set; }
    public decimal? LateDeductionHour { get; set; }
    public int? LateDeductionDays { get; set; }
    public decimal? OvertimeSalaryPercentage { get; set; }
    public int? StoreId { get; set; }
    public string? StoreName { get; set; }
    public DateTime? CreatedOn { get; set; }  // Changed to nullable
    public string? CreatedBy { get; set; }
    public DateTime? ModifiedOn { get; set; }
    public string? ModifiedBy { get; set; }
}


