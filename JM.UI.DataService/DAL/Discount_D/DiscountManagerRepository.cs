using JM.Infrastructure.Models;
using JM.UI.Entities.Model.Discount_D;
using JM.UI.Entities.Services;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Net.Http.Json;
using System.Text;

namespace JM.UI.DataService.DAL.Discount_D
{
    public class DiscountManagerRepository : BaseRepository, IDiscountManagerRepository
    {
        public DiscountManagerRepository(
            IHttpClientFactory httpClientFactory,
            ITokenProvider tokenProvider,
            ILogger<DiscountManagerRepository> logger)
            : base(httpClientFactory, tokenProvider, logger)
        {
        }

        public async Task<IEnumerable<DiscountManagerDTO>> GetAll()
        {
            try
            {
                _logger.LogInformation("Starting to fetch all discount campaigns");

                var httpClient = GetAuthenticatedClient("MainApi");
                var response = await httpClient.GetAsync("api/DiscountManager/getall");
                response.EnsureSuccessStatusCode();

                var result = await response.Content.ReadFromJsonAsync<List<DiscountManagerDTO>>();

                return result ?? new List<DiscountManagerDTO>();
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP request failed during get all discount campaigns");
                throw new Exception("Failed to fetch discount campaigns: " + ex.Message, ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during get all discount campaigns");
                throw new Exception("Unexpected error fetching discount campaigns: " + ex.Message, ex);
            }
        }

        public async Task<DiscountManagerDTO?> GetById(int id)
        {
            try
            {
                _logger.LogInformation("Starting to fetch discount campaign: {Id}", id);

                var httpClient = GetAuthenticatedClient("MainApi");
                var response = await httpClient.GetAsync($"api/DiscountManager/get/{id}");

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Discount campaign not found: {Id}", id);
                    return null;
                }

                var result = await response.Content.ReadFromJsonAsync<DiscountManagerDTO>();
                return result;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP request failed during get discount campaign by ID: {Id}", id);
                throw new Exception($"Failed to fetch discount campaign: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during get discount campaign by ID: {Id}", id);
                throw new Exception($"Unexpected error fetching discount campaign: {ex.Message}", ex);
            }
        }

        public async Task<ResponseResult> SaveUpdate(DiscountManagerDTO discountManager)
        {
            try
            {
                _logger.LogInformation("Starting to save discount campaign");

                var httpClient = GetAuthenticatedClient("MainApi");
                var command = new
                {
                    DiscountManager = new
                    {
                        Id = discountManager.Id,
                        DiscountName = discountManager.DiscountName,
                        StartDate = discountManager.StartDate,
                        EndDate = discountManager.EndDate,
                        IsActive = discountManager.IsActive,
                        CreatedBy = discountManager.CreatedBy,
                        ModifiedBy = discountManager.ModifiedBy,
                        ModifiedDate = discountManager.ModifiedDate,
                        DiscountDetails = discountManager.DiscountDetails
                    }
                };
                var response = await httpClient.PostAsJsonAsync("api/DiscountManager/insert-update", command);
                response.EnsureSuccessStatusCode();

                var result = await response.Content.ReadFromJsonAsync<ResponseResult>();

                return result ?? new ResponseResult { IsSuccessStatus = false, Message = "No response from server" };
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP request failed during save discount campaign");
                throw new Exception("Failed to save discount campaign: " + ex.Message, ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during save discount campaign");
                throw new Exception("Unexpected error saving discount campaign: " + ex.Message, ex);
            }
        }

        public async Task<ResponseResult> Delete(int id)
        {
            try
            {
                _logger.LogInformation("Starting to delete discount campaign: {Id}", id);

                var httpClient = GetAuthenticatedClient("MainApi");
                var response = await httpClient.DeleteAsync($"api/DiscountManager/delete/{id}");
                response.EnsureSuccessStatusCode();

                var result = await response.Content.ReadFromJsonAsync<ResponseResult>();

                _logger.LogInformation("Discount campaign deleted successfully: {Id}", id);

                return result ?? new ResponseResult { IsSuccessStatus = true, Message = "Deleted successfully" };
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP request exception during delete discount campaign: {Id}", id);
                throw new Exception($"Failed to delete discount campaign: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during delete discount campaign: {Id}", id);
                throw new Exception($"Unexpected error deleting discount campaign: {ex.Message}", ex);
            }
        }

        public async Task<IEnumerable<DiscountTypeDTO>> GetDiscountTypes()
        {
            try
            {
                _logger.LogInformation("Starting to fetch discount types");

                var httpClient = GetAuthenticatedClient("MainApi");
                var response = await httpClient.GetAsync("api/DiscountManager/discount-types");
                response.EnsureSuccessStatusCode();

                var result = await response.Content.ReadFromJsonAsync<List<DiscountTypeDTO>>();

                return result ?? new List<DiscountTypeDTO>();
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP request failed during get discount types");
                throw new Exception("Failed to fetch discount types: " + ex.Message, ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during get discount types");
                throw new Exception("Unexpected error fetching discount types: " + ex.Message, ex);
            }
        }
    }
}
