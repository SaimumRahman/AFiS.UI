using JM.Infrastructure.Models;
using JM.UI.Entities.Model.Accounting_D;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace JM.UI.DataService.DAL.Accounting
{
    public interface IChartOfAccountRepository
    {
        Task<IEnumerable<ChartOfAccountDTO>> GetChartOfAccounts();
        Task<ChartOfAccountDTO?> GetChartOfAccountById(int accountId);
        Task<IEnumerable<ChartOfAccountDTO>> GetGroupAccounts();
        Task<IEnumerable<ChartOfAccountDTO>> GetExpenseAccounts();

        /// <summary>Active ledger accounts a financial account (cash / MFS / bank / POS) can post to.</summary>
        Task<IEnumerable<ChartOfAccountDTO>> GetLedgerAccounts();
        Task<IEnumerable<string>> GetPartyTypes();

        /// <summary>Preview of the account number a new head under the given parent would get.</summary>
        Task<NextAccountNoDTO?> GetNextAccountNo(int parentId, bool isGroup);

        Task<ResponseResult> SaveUpdateChartOfAccount(ChartOfAccountDTO account);
        Task<ResponseResult> DeleteChartOfAccount(int accountId);
    }
}
