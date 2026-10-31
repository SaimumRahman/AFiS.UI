using JM.Infrastructure.Models;
using JM.UI.Entities.Model.Accounting_D;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace JM.UI.DataService.DAL.Accounting
{
    public interface IOpeningBalanceRepository
    {
        Task<OpeningBalanceViewDTO?> GetOpeningBalance(string companyCode, string branchCode, int fiscalYearId);
        Task<IEnumerable<AccountingFiscalYearDTO>> GetFiscalYears(string companyCode);
        Task<IEnumerable<AccountingBranchDTO>> GetBranches(string companyCode);
        Task<IEnumerable<AccountingPartyDTO>> GetParties(string? partyType);
        Task<ResponseResult> PostOpeningBalance(OpeningBalanceRequestDTO request);
    }
}
