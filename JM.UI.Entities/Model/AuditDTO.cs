using System;
using System.Collections.Generic;
using System.Text;

namespace JM.UI.Entities.Model;

public class AuditDTO
{
    public DateTime? CreatedDate { get; set; }
    public int? CreatedBy { get; set; }
    public DateTime? UpdateDate { get; set; }
    public int? UpdateBy { get; set; }
}
