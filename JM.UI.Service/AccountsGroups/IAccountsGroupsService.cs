using JM.Infrastructure.Models;
using JM.UI.Entities.Model.AccountsGroups;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace JM.UI.Service.AccountsGroups
{
    public interface IAccountsGroupsService
    {
        Task<IEnumerable<AccountsGroupsModelDTO>> GetAccountsGroups();
        Task<AccountsGroupsModelDTO?> GetAccountsGroupsById(int id);
        Task<ResponseResult> SaveUpdateAccountsGroups(AccountsGroupsModelDTO accountsGroups);
        Task<ResponseResult> DeleteAccountsGroups(int id);
    }
}
