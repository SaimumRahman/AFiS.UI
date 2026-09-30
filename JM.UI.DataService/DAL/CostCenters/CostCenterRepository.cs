using JM.Infrastructure.Models;
using JM.UI.Entities.Model.CostCenters;
using JM.UI.Entities.Services;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Net.Http.Json;

namespace JM.UI.DataService.DAL.CostCenters
{
    public class CostCenterRepository : BaseRepository, ICostCenterRepository
    {
        public CostCenterRepository(
            IHttpClientFactory httpClientFactory,
            ITokenProvider tokenProvider,
            ILogger<CostCenterRepository> logger)
            : base(httpClientFactory, tokenProvider, logger)
        {
        }

        public async Task<IEnumerable<CostCenterDTO>> GetCostCenters(bool? onlyActive = null)
        {
            try
            {
                _logger.LogInformation("Starting to fetch all cost centers");

                var httpClient = GetAuthenticatedClient("MainApi");
                var url = onlyActive == null
                    ? "CostCenters/getall"
                    : $"CostCenters/getall?onlyActive={onlyActive.Value}";
                var response = await httpClient.GetAsync(url);
                response.EnsureSuccessStatusCode();

                var costCenters = await response.Content.ReadFromJsonAsync<List<CostCenterDTO>>();

                return costCenters ?? new List<CostCenterDTO>();
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP request failed during get cost centers");
                throw new Exception("Failed to fetch cost centers: " + ex.Message, ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during get cost centers");
                throw new Exception("Unexpected error fetching cost centers: " + ex.Message, ex);
            }
        }

        public async Task<CostCenterDTO?> GetCostCenterById(int id)
        {
            try
            {
                _logger.LogInformation("Starting to fetch cost center: {Id}", id);

                var httpClient = GetAuthenticatedClient("MainApi");
                var response = await httpClient.GetAsync($"CostCenters/get/{id}");

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Cost center not found: {Id}", id);
                    return null;
                }

                var costCenter = await response.Content.ReadFromJsonAsync<CostCenterDTO>();
                return costCenter;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP request failed during get cost center by ID: {Id}", id);
                throw new Exception($"Failed to fetch cost center: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during get cost center by ID: {Id}", id);
                throw new Exception($"Unexpected error fetching cost center: {ex.Message}", ex);
            }
        }

        public async Task<ResponseResult> SaveUpdateCostCenter(CostCenterDTO costCenter)
        {
            try
            {
                _logger.LogInformation("Starting to save cost center");

                var httpClient = GetAuthenticatedClient("MainApi");
                var requestBody = new
                {
                    CostCenter = costCenter
                };
                var content = JsonContent.Create(requestBody);
                var response = await httpClient.PostAsync("CostCenters/insert-update", content);
                response.EnsureSuccessStatusCode();

                var result = await response.Content.ReadFromJsonAsync<ResponseResult>();

                return result ?? new ResponseResult { IsSuccessStatus = false, Message = "No response from server" };
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP request failed during save cost center");
                throw new Exception("Failed to save cost center: " + ex.Message, ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during save cost center");
                throw new Exception("Unexpected error saving cost center: " + ex.Message, ex);
            }
        }

        public async Task DeleteCostCenter(int id)
        {
            try
            {
                _logger.LogInformation("Starting to deactivate cost center: {Id}", id);

                var httpClient = GetAuthenticatedClient("MainApi");
                var response = await httpClient.DeleteAsync($"CostCenters/delete/{id}");
                response.EnsureSuccessStatusCode();

                _logger.LogInformation("Cost center deactivated successfully: {Id}", id);
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP request exception during deactivate cost center: {Id}", id);
                throw new Exception($"Failed to deactivate cost center: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during deactivate cost center: {Id}", id);
                throw new Exception($"Unexpected error deactivating cost center: {ex.Message}", ex);
            }
        }
    }
}