using JM.Infrastructure.Models;
using JM.UI.Entities.Model.DailyExpense;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace JM.UI.Service.DailyExpense
{
    public interface IDailyExpenseService
    {
        Task<IEnumerable<DailyExpenseDTO>> GetDailyExpenses(DateTime fromDate, DateTime toDate, int? storeId);
        Task<DailyExpenseDTO?> GetDailyExpenseById(int id);
        Task<ResponseResult> SaveUpdateDailyExpense(DailyExpenseDTO dailyExpense);
        Task<ResponseResult> DeleteDailyExpense(int id, int? updatedBy);
        Task<(bool IsValid, string ErrorMessage)> ValidateDailyExpense(DailyExpenseDTO dto);
        Task<IEnumerable<DailyExpenseDTO>> GetDailyExpensesForPosting(DateTime businessDay, int? storeId);
        Task<ResponseResult> PostDailyExpenses(PostDailyExpensesDTO request);
        Task<ResponseResult> UnpostDailyExpense(UnpostDailyExpenseDTO request);
    }
}