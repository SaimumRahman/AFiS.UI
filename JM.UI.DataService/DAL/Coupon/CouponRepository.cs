using JM.Infrastructure.Models;
using JM.UI.Entities.Model.Coupon;
using JM.UI.Entities.Services;
using Microsoft.Extensions.Logging;
using System.Net.Http.Json;

namespace JM.UI.DataService.DAL.Coupon
{
    public class CouponRepository : BaseRepository, ICouponRepository
    {
        public CouponRepository(
            IHttpClientFactory httpClientFactory,
            ITokenProvider tokenProvider,
            ILogger<CouponRepository> logger)
            : base(httpClientFactory, tokenProvider, logger)
        {
        }

        public async Task<IEnumerable<CouponDTO>> GetAll()
        {
            try
            {
                _logger.LogInformation("Fetching all coupons");
                var httpClient = GetAuthenticatedClient("MainApi");
                var response = await httpClient.GetAsync("api/Coupon/getall");
                response.EnsureSuccessStatusCode();
                var result = await response.Content.ReadFromJsonAsync<List<CouponDTO>>();
                return result ?? new List<CouponDTO>();
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP request failed during get all coupons");
                throw new Exception("Failed to fetch coupons: " + ex.Message, ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during get all coupons");
                throw new Exception("Unexpected error fetching coupons: " + ex.Message, ex);
            }
        }

        public async Task<CouponDTO?> GetById(int id)
        {
            try
            {
                _logger.LogInformation("Fetching coupon: {Id}", id);
                var httpClient = GetAuthenticatedClient("MainApi");
                var response = await httpClient.GetAsync($"api/Coupon/get/{id}");
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Coupon not found: {Id}", id);
                    return null;
                }
                var result = await response.Content.ReadFromJsonAsync<CouponDTO>();
                return result;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP request failed during get coupon by ID: {Id}", id);
                throw new Exception($"Failed to fetch coupon: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during get coupon by ID: {Id}", id);
                throw new Exception($"Unexpected error fetching coupon: {ex.Message}", ex);
            }
        }

        public async Task<ResponseResult> SaveUpdate(CouponDTO coupon)
        {
            try
            {
                _logger.LogInformation("Saving coupon");
                var httpClient = GetAuthenticatedClient("MainApi");
                var response = await httpClient.PostAsJsonAsync("api/Coupon/insert-update", coupon);
                response.EnsureSuccessStatusCode();
                var result = await response.Content.ReadFromJsonAsync<ResponseResult>();
                return result ?? new ResponseResult { IsSuccessStatus = false, Message = "No response from server" };
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP request failed during save coupon");
                throw new Exception("Failed to save coupon: " + ex.Message, ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during save coupon");
                throw new Exception("Unexpected error saving coupon: " + ex.Message, ex);
            }
        }

        public async Task<ResponseResult> Delete(int id)
        {
            try
            {
                _logger.LogInformation("Deleting coupon: {Id}", id);
                var httpClient = GetAuthenticatedClient("MainApi");
                var response = await httpClient.DeleteAsync($"api/Coupon/delete/{id}");
                response.EnsureSuccessStatusCode();
                var result = await response.Content.ReadFromJsonAsync<ResponseResult>();
                return result ?? new ResponseResult { IsSuccessStatus = true, Message = "Deleted successfully" };
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP request exception during delete coupon: {Id}", id);
                throw new Exception($"Failed to delete coupon: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during delete coupon: {Id}", id);
                throw new Exception($"Unexpected error deleting coupon: {ex.Message}", ex);
            }
        }
    }
}
