using JM.Infrastructure.Models;
using JM.UI.DataService.DAL.UnitOfWork;
using JM.UI.Entities.Model.Accounting_D;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace JM.UI.Service.Accounting
{
    public class ChartOfAccountService : IChartOfAccountService
    {
        private readonly IRepositoryUnitOfWork _repositoryUnitOfWork;

        public ChartOfAccountService(IRepositoryUnitOfWork repositoryUnitOfWork)
            => _repositoryUnitOfWork = repositoryUnitOfWork;

        public async Task<IEnumerable<ChartOfAccountDTO>> GetChartOfAccounts()
            => await _repositoryUnitOfWork.ChartOfAccountRepository.GetChartOfAccounts();

        public async Task<ChartOfAccountDTO?> GetChartOfAccountById(int accountId)
            => await _repositoryUnitOfWork.ChartOfAccountRepository.GetChartOfAccountById(accountId);

        public async Task<IEnumerable<ChartOfAccountDTO>> GetGroupAccounts()
            => await _repositoryUnitOfWork.ChartOfAccountRepository.GetGroupAccounts();

        public async Task<IEnumerable<ChartOfAccountDTO>> GetExpenseAccounts()
            => await _repositoryUnitOfWork.ChartOfAccountRepository.GetExpenseAccounts();

        public async Task<IEnumerable<string>> GetPartyTypes()
            => await _repositoryUnitOfWork.ChartOfAccountRepository.GetPartyTypes();

        public async Task<ResponseResult> SaveUpdateChartOfAccount(ChartOfAccountDTO account)
        {
            var v = await ValidateChartOfAccount(account);
            if (!v.IsValid)
                return new ResponseResult { IsSuccessStatus = false, Message = v.ErrorMessage };

            return await _repositoryUnitOfWork.ChartOfAccountRepository.SaveUpdateChartOfAccount(account);
        }

        public async Task<ResponseResult> DeleteChartOfAccount(int accountId)
            => await _repositoryUnitOfWork.ChartOfAccountRepository.DeleteChartOfAccount(accountId);

        public Task<(bool IsValid, string ErrorMessage)> ValidateChartOfAccount(ChartOfAccountDTO dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Code))
                return Task.FromResult((false, "Account code is required"));

            if (string.IsNullOrWhiteSpace(dto.Name))
                return Task.FromResult((false, "Account name is required"));

            if (string.IsNullOrWhiteSpace(dto.Nature))
                return Task.FromResult((false, "Account nature is required"));

            if (dto.RequiresParty && (dto.PartyTypes == null || dto.PartyTypes.Count == 0))
                return Task.FromResult((false, "At least one party type is required when the account requires a party"));

            return Task.FromResult((true, string.Empty));
        }
    }
}
