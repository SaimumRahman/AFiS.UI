using JM.Infrastructure.Models;
using JM.UI.Entities.Model.Discount;
using JM.UI.Entities.Model.SalesPOS;
using JM.UI.Entities.Services;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Text;

namespace JM.UI.DataService.DAL.Discount
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
                        AcknowledgeConflicts = discountManager.AcknowledgeConflicts,
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

        public async Task<IEnumerable<ProductSearchDTO>> SearchCampaignProductsAsync(
            string term, IEnumerable<int> storeIds)
        {
            var stores = storeIds?.Distinct().ToList() ?? new List<int>();
            if (string.IsNullOrWhiteSpace(term) || stores.Count == 0)
                return new List<ProductSearchDTO>();

            try
            {
                _logger.LogInformation("Searching campaign products: {Term} in stores {Stores}", term, string.Join(",", stores));

                var httpClient = GetAuthenticatedClient("MainApi");
                var url = $"api/DiscountManager/search-products?term={Uri.EscapeDataString(term)}";
                foreach (var s in stores)
                    url += $"&storeIds={s}";

                var response = await httpClient.GetAsync(url);
                if (!response.IsSuccessStatusCode)
                    return new List<ProductSearchDTO>();

                var result = await response.Content.ReadFromJsonAsync<List<ProductSearchDTO>>();
                return result ?? new List<ProductSearchDTO>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching campaign products");
                return new List<ProductSearchDTO>();
            }
        }

        public async Task<IEnumerable<ProductSearchDTO>> SearchCampaignBarcodeAsync(
            string barcode, IEnumerable<int> storeIds)
        {
            var stores = storeIds?.Distinct().ToList() ?? new List<int>();
            if (string.IsNullOrWhiteSpace(barcode) || stores.Count == 0)
                return new List<ProductSearchDTO>();

            try
            {
                _logger.LogInformation("Scanning campaign barcode: {Barcode} in stores {Stores}", barcode, string.Join(",", stores));

                var httpClient = GetAuthenticatedClient("MainApi");
                var url = $"api/DiscountManager/search-barcode/{Uri.EscapeDataString(barcode)}";
                url += "?";
                foreach (var s in stores)
                    url += $"storeIds={s}&";
                url = url.TrimEnd('&');

                var response = await httpClient.GetAsync(url);
                if (!response.IsSuccessStatusCode)
                    return new List<ProductSearchDTO>();

                var result = await response.Content.ReadFromJsonAsync<List<ProductSearchDTO>>();
                return result ?? new List<ProductSearchDTO>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error scanning campaign barcode");
                return new List<ProductSearchDTO>();
            }
        }

        public async Task<IEnumerable<CampaignConflictDTO>> GetConflictsAsync(
            IEnumerable<int> itemIds,
            IEnumerable<int> storeIds,
            DateTime startDate,
            DateTime endDate,
            int excludeCampaignId)
        {
            try
            {
                var payload = new
                {
                    ItemIds = itemIds?.Distinct().ToList() ?? new List<int>(),
                    StoreIds = storeIds?.Distinct().ToList() ?? new List<int>(),
                    StartDate = startDate,
                    EndDate = endDate,
                    ExcludeCampaignId = excludeCampaignId
                };

                var httpClient = GetAuthenticatedClient("MainApi");
                var response = await httpClient.PostAsJsonAsync("api/DiscountManager/conflicts", payload);
                response.EnsureSuccessStatusCode();

                var result = await response.Content.ReadFromJsonAsync<List<CampaignConflictDTO>>();
                return result ?? new List<CampaignConflictDTO>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to check discount campaign conflicts");
                return new List<CampaignConflictDTO>();
            }
        }
    }
}
