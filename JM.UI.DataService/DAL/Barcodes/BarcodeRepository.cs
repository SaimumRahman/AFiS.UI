using JM.Infrastructure.Models;
using JM.UI.Entities.Model.Barcodes;
using JM.UI.Entities.Services;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace JM.UI.DataService.DAL.Barcodes
{
    public class BarcodeRepository : BaseRepository, IBarcodeRepository
    {
        public BarcodeRepository(
            IHttpClientFactory httpClientFactory,
            ITokenProvider tokenProvider,
            ILogger<BarcodeRepository> logger)
            : base(httpClientFactory, tokenProvider, logger)
        {
        }

        public async Task<IEnumerable<BarcodeModelDTO>> GetBarcodes()
        {
            try
            {
                _logger.LogInformation("Starting to fetch all barcodes");

                var httpClient = GetAuthenticatedClient("MainApi");
                var response = await httpClient.GetAsync("Barcodes/getall");
                response.EnsureSuccessStatusCode();

                var barcodes = await response.Content.ReadFromJsonAsync<List<BarcodeModelDTO>>();

                return barcodes ?? new List<BarcodeModelDTO>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during get barcodes");
                throw new Exception("Failed to fetch barcodes: " + ex.Message, ex);
            }
        }

        public async Task<BarcodeModelDTO?> GetBarcodeById(int id)
        {
            try
            {
                _logger.LogInformation("Starting to fetch barcode: {Id}", id);

                var httpClient = GetAuthenticatedClient("MainApi");
                var response = await httpClient.GetAsync($"Barcodes/get/{id}");

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Barcode not found: {Id}", id);
                    return null;
                }

                var barcode = await response.Content.ReadFromJsonAsync<BarcodeModelDTO>();
                return barcode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during get barcode by ID: {Id}", id);
                throw new Exception($"Failed to fetch barcode: {ex.Message}", ex);
            }
        }

        public async Task<ResponseResult> SaveUpdateBarcode(BarcodeModelDTO barcode)
        {
            try
            {
                _logger.LogInformation("Starting to save barcode");

                var httpClient = GetAuthenticatedClient("MainApi");
                var requestBody = new
                {
                    BarcodeDTO = barcode
                };
                var content = JsonContent.Create(requestBody);
                var response = await httpClient.PostAsync("Barcodes/insert-update", content);
                response.EnsureSuccessStatusCode();

                var result = await response.Content.ReadFromJsonAsync<ResponseResult>();

                return result ?? new ResponseResult { IsSuccessStatus = false, Message = "No response from server" };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during save barcode");
                throw new Exception("Failed to save barcode: " + ex.Message, ex);
            }
        }

        public async Task DeleteBarcode(int id)
        {
            try
            {
                _logger.LogInformation("Starting to delete barcode: {Id}", id);

                var httpClient = GetAuthenticatedClient("MainApi");
                var response = await httpClient.DeleteAsync($"Barcodes/delete/{id}");
                response.EnsureSuccessStatusCode();

                _logger.LogInformation("Barcode deleted successfully: {Id}", id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during delete barcode: {Id}", id);
                throw new Exception($"Failed to delete barcode: {ex.Message}", ex);
            }
        }
    }
}
