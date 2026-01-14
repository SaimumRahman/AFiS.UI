using JM.Infrastructure.Models;
using JM.UI.Entities.Model.Accounts;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace JM.UI.Service.Accounts
{
    public interface IAccountsService
    {
        Task<IEnumerable<AccountModelDTO>> GetAccounts();
        Task<AccountModelDTO?> GetAccountById(int id);
        Task<ResponseResult> SaveUpdateAccount(AccountModelDTO account);
        Task<ResponseResult> DeleteAccount(int id);
    }
}
