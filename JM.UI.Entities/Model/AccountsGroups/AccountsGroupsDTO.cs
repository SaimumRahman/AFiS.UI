using JM;
using JM.UI;
using JM.UI.Entities;
using JM.UI.Entities.Model;
using JM.UI.Entities.Model.Approval;
using JM.UI.Entities.Model.Company;
using System;
using System.Collections.Generic;
using System.Text;

namespace JM.UI.Entities.Model.AccountsGroups;


public class AccountsGroupsDTO
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int StoreId { get; set; }
    public string StoreName { get; set; } = string.Empty;
}

