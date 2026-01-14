using JM.Infrastructure.Models;
using JM.UI.DataService.DAL.UnitOfWork;
using JM.UI.Entities.Model.AccountsGroups;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Text;

namespace JM.UI.Service.AccountsGroups
{
    public class AccountsGroupsService : IAccountsGroupsService
    {
        private readonly IRepositoryUnitOfWork _unitOfWork;

        public AccountsGroupsService(IRepositoryUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IEnumerable<AccountsGroupsModelDTO>> GetAccountsGroups()
        {
            try
            {
                return await _unitOfWork.AccountsGroupsRepository.GetAccountsGroups();
            }
            catch (Exception)
            {
                throw;
            }


            return await _repositoryUnitOfWork.AccountsGroupsRepository.SaveUpdateAccountsGroups(AccountsGroups);
        }

        public async Task<AccountsGroupsModelDTO?> GetAccountsGroupsById(int id)
        {
            try
            {
                return await _unitOfWork.AccountsGroupsRepository.GetAccountsGroupsById(id);
            }
            catch (Exception)
            {
                throw;
            }

        public Task<(bool IsValid, string ErrorMessage)> ValidateAccountsGroups(AccountsGroupsDTO AccountsGroups)
        {
            if (string.IsNullOrWhiteSpace(AccountsGroups.Name))
                return Task.FromResult((false, "AccountsGroups name is required."));

            if (AccountsGroups.Name.Length > 100)
                return Task.FromResult((false, "AccountsGroups name cannot exceed 100 characters."));

            return Task.FromResult((true, string.Empty));
        }

        public async Task<ResponseResult> SaveUpdateAccountsGroups(AccountsGroupsModelDTO accountsGroups)
        {
            try
            {
                return await _unitOfWork.AccountsGroupsRepository.SaveUpdateAccountsGroups(accountsGroups);
            }
            catch (Exception ex)
            {
                return new ResponseResult { IsSuccessStatus = false, Message = ex.Message };
            }
        }

        public async Task<ResponseResult> DeleteAccountsGroups(int id)
        {
            try
            {
                await _unitOfWork.AccountsGroupsRepository.DeleteAccountsGroups(id);
                return new ResponseResult { IsSuccessStatus = true, Message = "Accounts Group deleted successfully." };
            }
            catch (Exception ex)
            {
                return new ResponseResult { IsSuccessStatus = false, Message = ex.Message };
            }
        }
    }
}
