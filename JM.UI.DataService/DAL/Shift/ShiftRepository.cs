using JM.Infrastructure.Models;
using JM.UI.Entities.Model.Shift;
using JM.UI.Entities.Services;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Net.Http.Json;
using System.Text;

namespace JM.UI.DataService.DAL.Shift
{
    public class ShiftRepository : BaseRepository, IShiftRepository
    {
        public ShiftRepository(
            IHttpClientFactory httpClientFactory,
            ITokenProvider tokenProvider,
            ILogger<ShiftRepository> logger)
            : base(httpClientFactory, tokenProvider, logger)
        {
        }

        public async Task<IEnumerable<ShiftDTO>> GetShift()
        {
            try
            {
                _logger.LogInformation("Service: Starting to fetch all Shift");
                var httpClient = GetAuthenticatedClient("MainApi");
                var response = await httpClient.GetAsync("Shift/getall");

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError($"Service: API returned {response.StatusCode}: {errorContent}");
                    throw new HttpRequestException($"API returned {response.StatusCode}");
                }

                var shifts = await response.Content.ReadFromJsonAsync<List<ShiftDTO>>();
                _logger.LogInformation($"Service: Retrieved {shifts?.Count ?? 0} shifts");

                return shifts ?? new List<ShiftDTO>();
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Service: HTTP request failed during get Shift");
                throw new Exception("Failed to fetch Shift: " + ex.Message, ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Service: Unexpected error during get Shift");
                throw new Exception("Unexpected error fetching Shift: " + ex.Message, ex);
            }
        }
        public async Task<ShiftDTO?> GetShiftById(int id)
        {
            try
            {
                _logger.LogInformation("Starting to fetch Shift: {Id}", id);

                var httpClient = GetAuthenticatedClient("MainApi");
                var response = await httpClient.GetAsync($"Shift/get/{id}");

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Shift not found: {Id}", id);
                    return null;
                }

                var Shift = await response.Content.ReadFromJsonAsync<ShiftDTO>();
                return Shift;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP request failed during get Shift by ID: {Id}", id);
                throw new Exception($"Failed to fetch Shift: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during get Shift by ID: {Id}", id);
                throw new Exception($"Unexpected error fetching Shift: {ex.Message}", ex);
            }
        }

        public async Task DeleteShift(int id)
        {
            try
            {
                _logger.LogInformation("Starting to delete Shift: {Id}", id);

                var httpClient = GetAuthenticatedClient("MainApi");
                var response = await httpClient.DeleteAsync($"Shift/delete/{id}");
                response.EnsureSuccessStatusCode();

                _logger.LogInformation("Shift deleted successfully: {Id}", id);
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP request exception during delete Shift: {Id}", id);
                throw new Exception($"Failed to delete Shift: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during delete Shift: {Id}", id);
                throw new Exception($"Unexpected error deleting Shift: {ex.Message}", ex);
            }
        }

        public async Task<ResponseResult> SaveUpdateShift(ShiftDTO Shift)
        {
            try
            {
                _logger.LogInformation("Starting to save Shift");

                var httpClient = GetAuthenticatedClient("MainApi");
                var requestBody = new
                {
                    ShiftDTO = Shift
                };
                var content = JsonContent.Create(requestBody);
                var response = await httpClient.PostAsync("Shift/insert-update", content);
                response.EnsureSuccessStatusCode();

                var result = await response.Content.ReadFromJsonAsync<ResponseResult>();

                return result ?? new ResponseResult { IsSuccessStatus = false, Message = "No response from server" };
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP request failed during save Shift");
                throw new Exception("Failed to save Shift: " + ex.Message, ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during save Shift");
                throw new Exception("Unexpected error saving Shift: " + ex.Message, ex);
            }
        }

    }
}
