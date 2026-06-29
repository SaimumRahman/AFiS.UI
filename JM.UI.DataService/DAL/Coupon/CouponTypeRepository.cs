using JM.Infrastructure.Models;
using JM.UI.Entities.Model.Coupon;
using JM.UI.Entities.Services;
using Microsoft.Extensions.Logging;
using System.Net.Http.Json;

namespace JM.UI.DataService.DAL.Coupon
{
    public class CouponTypeRepository : BaseRepository, ICouponTypeRepository
    {
        public CouponTypeRepository(
            IHttpClientFactory httpClientFactory,
            ITokenProvider tokenProvider,
            ILogger<CouponTypeRepository> logger)
            : base(httpClientFactory, tokenProvider, logger)
        {
        }

        public async Task<IEnumerable<CouponTypeDTO>> GetAll()
        {
            try
            {
                _logger.LogInformation("Fetching all coupon types");
                var httpClient = GetAuthenticatedClient("MainApi");
                var response = await httpClient.GetAsync("api/Coupon/coupon-types");
                response.EnsureSuccessStatusCode();
                var result = await response.Content.ReadFromJsonAsync<List<CouponTypeDTO>>();
                return result ?? new List<CouponTypeDTO>();
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP request failed during get all coupon types");
                throw new Exception("Failed to fetch coupon types: " + ex.Message, ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during get all coupon types");
                throw new Exception("Unexpected error fetching coupon types: " + ex.Message, ex);
            }
        }

        public async Task<CouponTypeDTO?> GetById(int id)
        {
            try
            {
                _logger.LogInformation("Fetching coupon type: {Id}", id);
                var httpClient = GetAuthenticatedClient("MainApi");
                var response = await httpClient.GetAsync($"api/Coupon/coupon-type/{id}");
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Coupon type not found: {Id}", id);
                    return null;
                }
                var result = await response.Content.ReadFromJsonAsync<CouponTypeDTO>();
                return result;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP request failed during get coupon type by ID: {Id}", id);
                throw new Exception($"Failed to fetch coupon type: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during get coupon type by ID: {Id}", id);
                throw new Exception($"Unexpected error fetching coupon type: {ex.Message}", ex);
            }
        }

        public async Task<ResponseResult> SaveUpdate(CouponTypeDTO couponType)
        {
            try
            {
                _logger.LogInformation("Saving coupon type");
                var httpClient = GetAuthenticatedClient("MainApi");
                var command = new { Id = couponType.Id, TypeName = couponType.TypeName };
                var response = await httpClient.PostAsJsonAsync("api/Coupon/coupon-type/insert-update", command);
                response.EnsureSuccessStatusCode();
                var result = await response.Content.ReadFromJsonAsync<ResponseResult>();
                return result ?? new ResponseResult { IsSuccessStatus = false, Message = "No response from server" };
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP request failed during save coupon type");
                throw new Exception("Failed to save coupon type: " + ex.Message, ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during save coupon type");
                throw new Exception("Unexpected error saving coupon type: " + ex.Message, ex);
            }
        }

        public async Task<ResponseResult> Delete(int id)
        {
            try
            {
                _logger.LogInformation("Deleting coupon type: {Id}", id);
                var httpClient = GetAuthenticatedClient("MainApi");
                var response = await httpClient.DeleteAsync($"api/Coupon/coupon-type/delete/{id}");
                response.EnsureSuccessStatusCode();
                var result = await response.Content.ReadFromJsonAsync<ResponseResult>();
                return result ?? new ResponseResult { IsSuccessStatus = true, Message = "Deleted successfully" };
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP request exception during delete coupon type: {Id}", id);
                throw new Exception($"Failed to delete coupon type: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during delete coupon type: {Id}", id);
                throw new Exception($"Unexpected error deleting coupon type: {ex.Message}", ex);
            }
        }
    }
}
