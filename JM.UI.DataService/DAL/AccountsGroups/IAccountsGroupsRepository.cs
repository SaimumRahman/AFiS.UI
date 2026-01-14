using JM.Infrastructure.Models;
using JM.UI.Entities.Model.AccountsGroups;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace JM.UI.DataService.DAL.AccountsGroups
{
    public interface IAccountsGroupsRepository
    {
        Task<IEnumerable<AccountsGroupsModelDTO>> GetAccountsGroups();
        Task<AccountsGroupsModelDTO?> GetAccountsGroupsById(int id);
        Task<ResponseResult> SaveUpdateAccountsGroups(AccountsGroupsModelDTO accountsGroups);
        Task DeleteAccountsGroups(int id);
    }
}
