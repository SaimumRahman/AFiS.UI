using JM.Infrastructure.Models;
using JM.UI.DataService.DAL.UnitOfWork;
using JM.UI.Entities.Model.Accounts;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace JM.UI.Service.Accounts
{
    public class AccountsService : IAccountsService
    {
        private readonly IRepositoryUnitOfWork _unitOfWork;

        public AccountsService(IRepositoryUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IEnumerable<AccountModelDTO>> GetAccounts()
        {
            try
            {
                return await _unitOfWork.AccountsRepository.GetAccounts();
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<AccountModelDTO?> GetAccountById(int id)
        {
            try
            {
                return await _unitOfWork.AccountsRepository.GetAccountById(id);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<ResponseResult> SaveUpdateAccount(AccountModelDTO account)
        {
            try
            {
                return await _unitOfWork.AccountsRepository.SaveUpdateAccount(account);
            }
            catch (Exception ex)
            {
                return new ResponseResult { IsSuccessStatus = false, Message = ex.Message };
            }
        }

        public async Task<ResponseResult> DeleteAccount(int id)
        {
            try
            {
                await _unitOfWork.AccountsRepository.DeleteAccount(id);
                return new ResponseResult { IsSuccessStatus = true, Message = "Account deleted successfully." };
            }
            catch (Exception ex)
            {
                return new ResponseResult { IsSuccessStatus = false, Message = ex.Message };
            }
        }
    }
}
