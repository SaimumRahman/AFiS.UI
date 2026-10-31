using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace JM.UI.Entities.Model;

public class AuditModelDTO
{
    [JsonIgnore]
    public int? CreatedBy { get; set; }

    [JsonIgnore]
    public DateTime? CreatedDate { get; set; }

    [JsonIgnore]
    public int? UpdatedBy { get; set; }

    [JsonIgnore]
    public DateTime? UpdatedDate { get; set; }

    [JsonIgnore]
    public bool IsDeletedBy { get; set; }

    [JsonIgnore]
    public int? DeletedBy { get; set; }

    [JsonIgnore]
    public DateTime? DeletedDate { get; set; }
}
