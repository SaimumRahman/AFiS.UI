using JM.Infrastructure.Models;
using JM.UI.DataService.DAL.UnitOfWork;
using JM.UI.Entities.Model.DailyExpense;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace JM.UI.Service.DailyExpense
{
    public class DailyExpenseService : IDailyExpenseService
    {
        private readonly IRepositoryUnitOfWork _repositoryUnitOfWork;

        public DailyExpenseService(IRepositoryUnitOfWork repositoryUnitOfWork)
            => _repositoryUnitOfWork = repositoryUnitOfWork;

        public async Task<IEnumerable<DailyExpenseDTO>> GetDailyExpenses(DateTime fromDate, DateTime toDate, int? storeId)
            => await _repositoryUnitOfWork.DailyExpenseRepository.GetDailyExpenses(fromDate, toDate, storeId);

        public async Task<DailyExpenseDTO?> GetDailyExpenseById(int id)
            => await _repositoryUnitOfWork.DailyExpenseRepository.GetDailyExpenseById(id);

        public async Task<IEnumerable<BillTypeDTO>> GetBillTypesForDropdown()
            => await _repositoryUnitOfWork.DailyExpenseRepository.GetBillTypesForDropdown();

        public async Task<ResponseResult> SaveUpdateDailyExpense(DailyExpenseDTO dailyExpense)
        {
            var v = await ValidateDailyExpense(dailyExpense);
            if (!v.IsValid)
                return new() { IsSuccessStatus = false, Message = v.ErrorMessage };

            return await _repositoryUnitOfWork.DailyExpenseRepository.SaveUpdateDailyExpense(dailyExpense);
        }

        public async Task<ResponseResult> DeleteDailyExpense(int id, int? updatedBy)
            => await _repositoryUnitOfWork.DailyExpenseRepository.DeleteDailyExpense(id, updatedBy);

        public Task<(bool IsValid, string ErrorMessage)> ValidateDailyExpense(DailyExpenseDTO dto)
        {
            if (dto.ExpenseDate == default)
                return Task.FromResult((false, "Expense date is required"));

            if (dto.BillTypeId <= 0)
                return Task.FromResult((false, "Bill type is required"));

            if (dto.StoreId <= 0)
                return Task.FromResult((false, "Store is required"));

            if (dto.Amount <= 0)
                return Task.FromResult((false, "Amount must be greater than zero"));

            if (string.IsNullOrWhiteSpace(dto.Description))
                return Task.FromResult((false, "Description is required"));

            return Task.FromResult((true, string.Empty));
        }
    }
}