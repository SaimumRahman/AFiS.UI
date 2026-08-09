using JM.Infrastructure.Models;
using JM.UI.Entities.Model.Bank;
using JM.UI.Entities.Model.FinancialAccounts;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace JM.UI.DataService.DAL.FinancialAccounts
{
    public interface IFinancialAccountsRepository
    {
        Task<IEnumerable<FinancialAccountDTO>> GetFinancialAccounts();
        Task<FinancialAccountDTO?> GetFinancialAccountById(int id);
        Task<ResponseResult> SaveUpdateFinancialAccount(FinancialAccountDTO financialAccount);
        Task<ResponseResult> DeleteFinancialAccount(int id);
        Task<IEnumerable<FinancialAccountTypeDTO>> GetFinancialAccountTypes();
        Task<IEnumerable<MFSTypeDTO>> GetMFSTypes();
        Task<IEnumerable<BanksDTO>> GetBanks();
    }
}