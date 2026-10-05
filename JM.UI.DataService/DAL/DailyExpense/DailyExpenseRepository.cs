using JM.Infrastructure.Models;
using JM.UI.Entities.Model.DailyExpense;
using JM.UI.Entities.Services;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace JM.UI.DataService.DAL.DailyExpense
{
    public class DailyExpenseRepository : BaseRepository, IDailyExpenseRepository
    {
        private readonly ILogger<DailyExpenseRepository> _logger;

        public DailyExpenseRepository(
            IHttpClientFactory httpClientFactory,
            ITokenProvider tokenProvider,
            ILogger<DailyExpenseRepository> logger)
            : base(httpClientFactory, tokenProvider, logger)
        {
            _logger = logger;
        }

        public async Task<IEnumerable<DailyExpenseDTO>> GetDailyExpenses(DateTime fromDate, DateTime toDate, int? storeId)
        {
            try
            {
                var httpClient = GetAuthenticatedClient("MainApi");
                var response = await httpClient.GetAsync(
                    $"DailyExpenses/GetAllDailyExpenses?fromDate={fromDate:yyyy-MM-dd}&toDate={toDate:yyyy-MM-dd}&storeId={storeId}");
                response.EnsureSuccessStatusCode();

                var result = await response.Content.ReadFromJsonAsync<List<DailyExpenseDTO>>();
                return result ?? new List<DailyExpenseDTO>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching daily expenses");
                throw;
            }
        }

        public async Task<DailyExpenseDTO?> GetDailyExpenseById(int id)
        {
            try
            {
                var httpClient = GetAuthenticatedClient("MainApi");
                var response = await httpClient.GetAsync($"DailyExpenses/GetDailyExpenseById/{id}");

                if (!response.IsSuccessStatusCode)
                    return null;

                return await response.Content.ReadFromJsonAsync<DailyExpenseDTO>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching daily expense by ID: {Id}", id);
                throw;
            }
        }

        public async Task<IEnumerable<BillTypeDTO>> GetBillTypesForDropdown()
        {
            try
            {
                var httpClient = GetAuthenticatedClient("MainApi");
                var response = await httpClient.GetAsync("DailyExpenses/GetBillTypesForDropdown");
                response.EnsureSuccessStatusCode();

                var result = await response.Content.ReadFromJsonAsync<List<BillTypeDTO>>();
                return result ?? new List<BillTypeDTO>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching bill types");
                throw;
            }
        }

        public async Task<ResponseResult> SaveUpdateDailyExpense(DailyExpenseDTO dailyExpense)
        {
            try
            {
                var httpClient = GetAuthenticatedClient("MainApi");
                var requestBody = new { DailyExpense = dailyExpense };
                var response = await httpClient.PostAsJsonAsync("DailyExpenses/InsertUpdateDailyExpense", requestBody);
                response.EnsureSuccessStatusCode();

                var result = await response.Content.ReadFromJsonAsync<ResponseResult>();
                return result ?? new ResponseResult { IsSuccessStatus = false, Message = "No response from server" };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving daily expense");
                throw;
            }
        }

        public async Task<ResponseResult> DeleteDailyExpense(int id, int? updatedBy)
        {
            try
            {
                var httpClient = GetAuthenticatedClient("MainApi");
                var response = await httpClient.DeleteAsync($"DailyExpenses/DeleteDailyExpense/{id}?updatedBy={updatedBy}");
                response.EnsureSuccessStatusCode();

                return await response.Content.ReadFromJsonAsync<ResponseResult>()
                    ?? new ResponseResult { IsSuccessStatus = false, Message = "No response from server" };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting daily expense: {Id}", id);
                return new ResponseResult { IsSuccessStatus = false, Message = ex.Message };
            }
        }

        public async Task<IEnumerable<DailyExpenseDTO>> GetDailyExpensesForPosting(DateTime businessDay, int? storeId)
        {
            try
            {
                var httpClient = GetAuthenticatedClient("MainApi");
                var url = $"DailyExpenses/GetDailyExpensesForPosting?businessDay={businessDay:yyyy-MM-dd}"
                        + (storeId.HasValue ? $"&storeId={storeId.Value}" : string.Empty);

                var response = await httpClient.GetAsync(url);
                response.EnsureSuccessStatusCode();

                var result = await response.Content.ReadFromJsonAsync<List<DailyExpenseDTO>>();
                return result ?? new List<DailyExpenseDTO>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching expenses for business day {Day}", businessDay);
                return new List<DailyExpenseDTO>();
            }
        }

        public async Task<ResponseResult> PostDailyExpenses(PostDailyExpensesDTO request)
        {
            try
            {
                var httpClient = GetAuthenticatedClient("MainApi");
                var response = await httpClient.PostAsJsonAsync("DailyExpenses/PostDailyExpenses", new { Request = request });
                response.EnsureSuccessStatusCode();

                return await response.Content.ReadFromJsonAsync<ResponseResult>()
                    ?? new ResponseResult { IsSuccessStatus = false, Message = "No response from server" };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error posting daily expenses to the ledger");
                return new ResponseResult { IsSuccessStatus = false, Message = ex.Message };
            }
        }

        public async Task<ResponseResult> UnpostDailyExpense(UnpostDailyExpenseDTO request)
        {
            try
            {
                var httpClient = GetAuthenticatedClient("MainApi");
                var response = await httpClient.PostAsJsonAsync("DailyExpenses/UnpostDailyExpense", new { Request = request });
                response.EnsureSuccessStatusCode();

                return await response.Content.ReadFromJsonAsync<ResponseResult>()
                    ?? new ResponseResult { IsSuccessStatus = false, Message = "No response from server" };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error unposting daily expense: {Id}", request?.Id);
                return new ResponseResult { IsSuccessStatus = false, Message = ex.Message };
            }
        }
    }
}