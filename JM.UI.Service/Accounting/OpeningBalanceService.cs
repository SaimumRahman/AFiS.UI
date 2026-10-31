using JM.Infrastructure.Models;
using JM.UI.DataService.DAL.UnitOfWork;
using JM.UI.Entities.Model.Accounting_D;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace JM.UI.Service.Accounting
{
    public class OpeningBalanceService : IOpeningBalanceService
    {
        private readonly IRepositoryUnitOfWork _repositoryUnitOfWork;

        public OpeningBalanceService(IRepositoryUnitOfWork repositoryUnitOfWork)
            => _repositoryUnitOfWork = repositoryUnitOfWork;

        public async Task<OpeningBalanceViewDTO?> GetOpeningBalance(string companyCode, string branchCode, int fiscalYearId)
            => await _repositoryUnitOfWork.OpeningBalanceRepository.GetOpeningBalance(companyCode, branchCode, fiscalYearId);

        public async Task<IEnumerable<AccountingFiscalYearDTO>> GetFiscalYears(string companyCode)
            => await _repositoryUnitOfWork.OpeningBalanceRepository.GetFiscalYears(companyCode);

        public async Task<IEnumerable<AccountingBranchDTO>> GetBranches(string companyCode)
            => await _repositoryUnitOfWork.OpeningBalanceRepository.GetBranches(companyCode);

        public async Task<IEnumerable<AccountingPartyDTO>> GetParties(string? partyType)
            => await _repositoryUnitOfWork.OpeningBalanceRepository.GetParties(partyType);

        public async Task<ResponseResult> PostOpeningBalance(OpeningBalanceRequestDTO request)
            => await _repositoryUnitOfWork.OpeningBalanceRepository.PostOpeningBalance(request);
    }
}
