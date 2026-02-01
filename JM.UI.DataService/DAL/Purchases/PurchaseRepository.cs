using JM.Infrastructure.Models;
using JM.UI.DataService.DAL.UnitOfWork;
using JM.UI.Entities.Model.PurchaseItems;
using JM.UI.Entities.Model.Purchases;
using JM.UI.Entities.Services;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace JM.UI.DataService.DAL.Purchases
{
    public class PurchaseRepository : BaseRepository, IPurchaseRepository
    {
        public PurchaseRepository(
            IHttpClientFactory httpClientFactory,
            ITokenProvider tokenProvider,
            ILogger<PurchaseRepository> logger)
            : base(httpClientFactory, tokenProvider, logger)
        {
        }

        // =============================================
        // Get All Purchases
        // =============================================
        public async Task<IEnumerable<PurchaseSummaryDTO>> GetPurchases()
        {
            try
            {
                _logger.LogInformation("Starting to fetch all purchases");

                var httpClient = GetAuthenticatedClient("MainApi");
                var response = await httpClient.GetAsync("api/Purchase/getall");
                response.EnsureSuccessStatusCode();

                var purchases = await response.Content.ReadFromJsonAsync<List<PurchaseSummaryDTO>>();
                return purchases ?? new List<PurchaseSummaryDTO>();
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP request failed during get purchases");
                throw new Exception("Failed to fetch purchases: " + ex.Message, ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during get purchases");
                throw new Exception("Unexpected error fetching purchases: " + ex.Message, ex);
            }
        }

        // =============================================
        // Get Purchase By Id
        // =============================================
        public async Task<PurchaseDTO?> GetPurchaseById(int id)
        {
            try
            {
                _logger.LogInformation("Starting to fetch purchase: {Id}", id);

                var httpClient = GetAuthenticatedClient("MainApi");
                var response = await httpClient.GetAsync($"api/Purchase/get/{id}");

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Purchase not found: {Id}", id);
                    return null;
                }

                var purchase = await response.Content.ReadFromJsonAsync<PurchaseDTO>();
                return purchase;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP request failed during get purchase by ID: {Id}", id);
                throw new Exception($"Failed to fetch purchase: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during get purchase by ID: {Id}", id);
                throw new Exception($"Unexpected error fetching purchase: {ex.Message}", ex);
            }
        }

        // =============================================
        // Save/Update Purchase
        // =============================================
        public async Task<ResponseResult> SaveUpdatePurchase(PurchaseDTO purchase, List<PurchaseItemDTO> items)
        {
            try
            {
                _logger.LogInformation("Starting to save purchase");

                var httpClient = GetAuthenticatedClient("MainApi");
                var requestBody = new
                {
                    Purchase = purchase,
                    Items = items
                };
                var content = JsonContent.Create(requestBody);
                var response = await httpClient.PostAsync("api/Purchase/insert-update", content);
                response.EnsureSuccessStatusCode();

                var result = await response.Content.ReadFromJsonAsync<ResponseResult>();
                return result ?? new ResponseResult { IsSuccessStatus = false, Message = "No response from server" };
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP request failed during save purchase");
                throw new Exception("Failed to save purchase: " + ex.Message, ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during save purchase");
                throw new Exception("Unexpected error saving purchase: " + ex.Message, ex);
            }
        }

        // =============================================
        // Delete Purchase
        // =============================================
        public async Task DeletePurchase(int id)
        {
            try
            {
                _logger.LogInformation("Starting to delete purchase: {Id}", id);

                var httpClient = GetAuthenticatedClient("MainApi");
                var response = await httpClient.DeleteAsync($"api/Purchase/delete/{id}");
                response.EnsureSuccessStatusCode();

                _logger.LogInformation("Purchase deleted successfully: {Id}", id);
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP request exception during delete purchase: {Id}", id);
                throw new Exception($"Failed to delete purchase: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during delete purchase: {Id}", id);
                throw new Exception($"Unexpected error deleting purchase: {ex.Message}", ex);
            }
        }

        // =============================================
        // Generate Barcode
        // =============================================
        public async Task<string> GenerateBarcode(BarcodeGenerationRequestDTO request)
        {
            try
            {
                _logger.LogInformation("Generating barcode");

                var httpClient = GetAuthenticatedClient("MainApi");
                var content = JsonContent.Create(request);
                var response = await httpClient.PostAsync("api/Purchase/generate-barcode", content);
                response.EnsureSuccessStatusCode();

                var result = await response.Content.ReadFromJsonAsync<BarcodeResponse>();
                return result?.Barcode ?? string.Empty;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating barcode");
                throw new Exception("Failed to generate barcode: " + ex.Message, ex);
            }
        }

        // =============================================
        // Search By Barcode
        // =============================================
        public async Task<BarcodeSearchResponseDTO> SearchByBarcode(string barcode)
        {
            try
            {
                _logger.LogInformation("Searching by barcode: {Barcode}", barcode);

                var httpClient = GetAuthenticatedClient("MainApi");
                var response = await httpClient.GetAsync($"api/Purchase/search-barcode/{barcode}");
                response.EnsureSuccessStatusCode();

                var result = await response.Content.ReadFromJsonAsync<BarcodeSearchResponseDTO>();
                return result ?? new BarcodeSearchResponseDTO { Found = false };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching by barcode: {Barcode}", barcode);
                throw new Exception("Failed to search by barcode: " + ex.Message, ex);
            }
        }
    }
    internal class BarcodeResponse
    {
        public string Barcode { get; set; } = string.Empty;
    }
}
