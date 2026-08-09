using JM.Infrastructure.Models;
using JM.UI.DataService.DAL.UnitOfWork;
using JM.UI.Entities.Model.Bank;
using JM.UI.Entities.Model.FinancialAccounts;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace JM.UI.Service.FinancialAccounts
{
    public class FinancialAccountsService : IFinancialAccountsService
    {
        private readonly IRepositoryUnitOfWork _repositoryUnitOfWork;

        public FinancialAccountsService(IRepositoryUnitOfWork repositoryUnitOfWork)
            => _repositoryUnitOfWork = repositoryUnitOfWork;

        public async Task<IEnumerable<FinancialAccountDTO>> GetFinancialAccounts()
            => await _repositoryUnitOfWork.FinancialAccountsRepository.GetFinancialAccounts();

        public async Task<FinancialAccountDTO?> GetFinancialAccountById(int id)
            => await _repositoryUnitOfWork.FinancialAccountsRepository.GetFinancialAccountById(id);

        public async Task<ResponseResult> SaveUpdateFinancialAccount(FinancialAccountDTO financialAccount)
        {
            var v = await ValidateFinancialAccount(financialAccount);
            if (!v.IsValid)
                return new() { IsSuccessStatus = false, Message = v.ErrorMessage };

            return await _repositoryUnitOfWork.FinancialAccountsRepository.SaveUpdateFinancialAccount(financialAccount);
        }

        public async Task<ResponseResult> DeleteFinancialAccount(int id)
            => await _repositoryUnitOfWork.FinancialAccountsRepository.DeleteFinancialAccount(id);

        public async Task<IEnumerable<FinancialAccountTypeDTO>> GetFinancialAccountTypes()
            => await _repositoryUnitOfWork.FinancialAccountsRepository.GetFinancialAccountTypes();

        public async Task<IEnumerable<MFSTypeDTO>> GetMFSTypes()
            => await _repositoryUnitOfWork.FinancialAccountsRepository.GetMFSTypes();

        public async Task<IEnumerable<BanksDTO>> GetBanks()
            => await _repositoryUnitOfWork.FinancialAccountsRepository.GetBanks();

        public async Task<IEnumerable<FinancialAccountDropdownDTO>> GetFinancialAccountsForDropdown()
            => await _repositoryUnitOfWork.FinancialAccountsRepository.GetFinancialAccountsForDropdown();

        public Task<(bool IsValid, string ErrorMessage)> ValidateFinancialAccount(FinancialAccountDTO dto)
        {
            if (dto.FinancialAccountTypeId <= 0)
                return Task.FromResult((false, "Financial Account Type is required"));

            if (string.IsNullOrWhiteSpace(dto.AccountNo))
                return Task.FromResult((false, "Account Number is required"));

            return Task.FromResult((true, string.Empty));
        }

        public string Truncate(string? value, int maxChars)
            => value?.Length > maxChars ? value[..maxChars] + "..." : value ?? "";
    }
}