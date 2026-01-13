using JM.Infrastructure.Models;
using JM.UI.DataService.DAL.UnitOfWork;
using JM.UI.Entities.Model.AccountsGroups;
using System;
using System.Collections.Generic;
using System.Text;

namespace JM.UI.Service.AccountsGroups
{
    public class AccountsGroupsService : IAccountsGroupsService
    {
        private readonly IRepositoryUnitOfWork _repositoryUnitOfWork;

        public AccountsGroupsService(IRepositoryUnitOfWork repositoryUnitOfWork)
            => _repositoryUnitOfWork = repositoryUnitOfWork;

        public async Task<IEnumerable<AccountsGroupsDTO>> GetAccountsGroups()
        {
            var companies = await _repositoryUnitOfWork.AccountsGroupsRepository.GetAccountsGroups();
            return companies.Select(c => new AccountsGroupsDTO
            {
                Id = c.Id,
                Name = c.Name,
                StoreId = c.StoreId,
                StoreName = c.StoreName

            }).ToList();
        }

        public async Task<AccountsGroupsDTO?> GetAccountsGroupsById(int id)
        {
            return await _repositoryUnitOfWork.AccountsGroupsRepository.GetAccountsGroupsById(id);
        }

        public async Task<ResponseResult> SaveUpdateAccountsGroups(AccountsGroupsDTO AccountsGroups)
        {
            var validation = await ValidateAccountsGroups(AccountsGroups);
            if (!validation.IsValid)
            {
                return new ResponseResult
                {
                    IsSuccessStatus = false,
                    Message = validation.ErrorMessage
                };
            }


            return await _repositoryUnitOfWork.AccountsGroupsRepository.SaveUpdateAccountsGroups(AccountsGroups);
        }

        public async Task<ResponseResult> DeleteAccountsGroups(int id)
        {
            try
            {
                await _repositoryUnitOfWork.AccountsGroupsRepository.DeleteAccountsGroups(id);
                return new ResponseResult
                {
                    IsSuccessStatus = true,
                    Message = "AccountsGroups deleted successfully!"
                };
            }
            catch (Exception ex)
            {
                return new ResponseResult
                {
                    IsSuccessStatus = false,
                    Message = $"Failed to delete AccountsGroups: {ex.Message}"
                };
            }
        }

        public Task<(bool IsValid, string ErrorMessage)> ValidateAccountsGroups(AccountsGroupsDTO AccountsGroups)
        {
            if (string.IsNullOrWhiteSpace(AccountsGroups.Name))
                return Task.FromResult((false, "AccountsGroups name is required."));

            if (AccountsGroups.Name.Length > 100)
                return Task.FromResult((false, "AccountsGroups name cannot exceed 100 characters."));

            return Task.FromResult((true, string.Empty));
        }

        private bool IsValidEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }

        public AccountsGroupsDTO CreateNewAccountsGroups()
        {
            return new AccountsGroupsDTO
            {
                //CreatedOn = DateTime.Now
            };
        }

        public string Truncate(string? value, int maxChars)
        {
            return value?.Length > maxChars ? value.Substring(0, maxChars) + "..." : value ?? string.Empty;
        }
    }
}
