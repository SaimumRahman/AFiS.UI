using JM;
using JM.Infrastructure.Models;
using JM.UI;
using JM.UI.DataService;
using JM.UI.DataService.DAL;
using JM.UI.DataService.DAL.Approval;
using JM.UI.DataService.DAL.AccountsGroups;
using JM.UI.Entities.Model.AccountsGroups;
using System;
using System.Collections.Generic;
using System.Text;

namespace JM.UI.DataService.DAL.AccountsGroups
{
    public interface IAccountsGroupsRepository
    {
        Task<IEnumerable<AccountsGroupsDTO>> GetAccountsGroups();
        Task<AccountsGroupsDTO?> GetAccountsGroupsById(int id);
        Task DeleteAccountsGroups(int id);
        Task<ResponseResult> SaveUpdateAccountsGroups(AccountsGroupsDTO AccountsGroups);
    }
}
