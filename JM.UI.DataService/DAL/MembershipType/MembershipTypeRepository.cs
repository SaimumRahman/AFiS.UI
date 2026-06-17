using JM.Infrastructure.Models;
using JM.UI.Entities.Model.MembershipType;
using JM.UI.Entities.Services;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Net.Http.Json;
using System.Text;
using System.Xml.Linq;

namespace JM.UI.DataService.DAL.MembershipType
{
    public class MembershipTypeRepository : BaseRepository, IMembershipTypeRepository
    {
        public MembershipTypeRepository(
            IHttpClientFactory httpClientFactory,
            ITokenProvider tokenProvider,
            ILogger<MembershipTypeRepository> logger)
            : base(httpClientFactory, tokenProvider, logger)
        {
        }

        public async Task<IEnumerable<MembershipTypeDTO>> GetAll()
        {
            try
            {
                _logger.LogInformation("Starting to fetch all membership types");

                var httpClient = GetAuthenticatedClient("MainApi");
                var response = await httpClient.GetAsync("MembershipTypes/getall");
                response.EnsureSuccessStatusCode();

                var membershipTypes = await response.Content.ReadFromJsonAsync<List<MembershipTypeDTO>>();

                return membershipTypes ?? new List<MembershipTypeDTO>();
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP request failed during get all membership types");
                throw new Exception("Failed to fetch membership types: " + ex.Message, ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during get all membership types");
                throw new Exception("Unexpected error fetching membership types: " + ex.Message, ex);
            }
        }

        public async Task<MembershipTypeDTO?> GetById(int id)
        {
            try
            {
                _logger.LogInformation("Starting to fetch membership type: {Id}", id);

                var httpClient = GetAuthenticatedClient("MainApi");
                var response = await httpClient.GetAsync($"MembershipTypes/get/{id}");

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Membership type not found: {Id}", id);
                    return null;
                }

                var membershipType = await response.Content.ReadFromJsonAsync<MembershipTypeDTO>();
                return membershipType;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP request failed during get membership type by ID: {Id}", id);
                throw new Exception($"Failed to fetch membership type: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during get membership type by ID: {Id}", id);
                throw new Exception($"Unexpected error fetching membership type: {ex.Message}", ex);
            }
        }

        public async Task<ResponseResult> SaveUpdate(MembershipTypeDTO membershipType)
        {
            try
            {
                _logger.LogInformation("Starting to save membership type");
                var command = new
                {
                    MembershipTypeDTO = new
                    {
                        Id = membershipType.Id,
                        Name = membershipType.Name,
                        DurationInMonths = membershipType.DurationInMonths,
                        DiscountRate = membershipType.DiscountRate
                    }
                };
                var httpClient = GetAuthenticatedClient("MainApi");
                var response = await httpClient.PostAsJsonAsync("MembershipTypes/insert-update", command);
                response.EnsureSuccessStatusCode();

                var result = await response.Content.ReadFromJsonAsync<ResponseResult>();

                return result ?? new ResponseResult { IsSuccessStatus = false, Message = "No response from server" };
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP request failed during save membership type");
                throw new Exception("Failed to save membership type: " + ex.Message, ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during save membership type");
                throw new Exception("Unexpected error saving membership type: " + ex.Message, ex);
            }
        }

        public async Task<ResponseResult> Delete(int id)
        {
            try
            {
                _logger.LogInformation("Starting to delete membership type: {Id}", id);

                var httpClient = GetAuthenticatedClient("MainApi");
                var response = await httpClient.DeleteAsync($"MembershipTypes/delete/{id}");
                response.EnsureSuccessStatusCode();

                var result = await response.Content.ReadFromJsonAsync<ResponseResult>();

                _logger.LogInformation("Membership type deleted successfully: {Id}", id);

                return result ?? new ResponseResult { IsSuccessStatus = true, Message = "Deleted successfully" };
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP request exception during delete membership type: {Id}", id);
                throw new Exception($"Failed to delete membership type: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during delete membership type: {Id}", id);
                throw new Exception($"Unexpected error deleting membership type: {ex.Message}", ex);
            }
        }
    }
}
