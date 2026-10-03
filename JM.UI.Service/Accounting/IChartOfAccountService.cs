using JM.Infrastructure.Models;
using JM.UI.Entities.Model.Accounting_D;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace JM.UI.Service.Accounting
{
    public interface IChartOfAccountService
    {
        Task<IEnumerable<ChartOfAccountDTO>> GetChartOfAccounts();
        Task<ChartOfAccountDTO?> GetChartOfAccountById(int accountId);
        Task<IEnumerable<ChartOfAccountDTO>> GetGroupAccounts();
        Task<IEnumerable<string>> GetPartyTypes();
        Task<ResponseResult> SaveUpdateChartOfAccount(ChartOfAccountDTO account);
        Task<ResponseResult> DeleteChartOfAccount(int accountId);
        Task<(bool IsValid, string ErrorMessage)> ValidateChartOfAccount(ChartOfAccountDTO dto);
    }
}
