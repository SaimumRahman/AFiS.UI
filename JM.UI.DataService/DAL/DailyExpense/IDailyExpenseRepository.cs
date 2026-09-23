using JM.Infrastructure.Models;
using JM.UI.Entities.Model.DailyExpense;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace JM.UI.DataService.DAL.DailyExpense
{
    public interface IDailyExpenseRepository
    {
        Task<IEnumerable<DailyExpenseDTO>> GetDailyExpenses(DateTime fromDate, DateTime toDate, int? storeId);
        Task<DailyExpenseDTO?> GetDailyExpenseById(int id);
        Task<IEnumerable<BillTypeDTO>> GetBillTypesForDropdown();
        Task<ResponseResult> SaveUpdateDailyExpense(DailyExpenseDTO dailyExpense);
        Task<ResponseResult> DeleteDailyExpense(int id, int? updatedBy);
    }
}