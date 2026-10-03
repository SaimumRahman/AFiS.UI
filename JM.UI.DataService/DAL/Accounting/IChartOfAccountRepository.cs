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
        Task<IEnumerable<string>> GetPartyTypes();
        Task<ResponseResult> SaveUpdateChartOfAccount(ChartOfAccountDTO account);
        Task<ResponseResult> DeleteChartOfAccount(int accountId);
    }
}
