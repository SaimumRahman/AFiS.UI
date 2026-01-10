using JM.Infrastructure.Models;
using JM.UI.Entities.Model.Company;
using JM.UI.Entities.Services;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Net.Http.Json;
using System.Text;

namespace JM.UI.DataService.DAL.Company
{
    public class CompanyRepository : BaseRepository, ICompanyRepository
    {
        public CompanyRepository(
            IHttpClientFactory httpClientFactory,
            ITokenProvider tokenProvider,
            ILogger<CompanyRepository> logger)
            : base(httpClientFactory, tokenProvider, logger)
        {
        }

        public async Task<IEnumerable<CompanyDTO>> GetCompanies()
        {
            try
            {
                _logger.LogInformation("Starting to fetch all companies");

                var httpClient = GetAuthenticatedClient("MainApi");
                var response = await httpClient.GetAsync("Companies/getall");
                response.EnsureSuccessStatusCode();

                var companies = await response.Content.ReadFromJsonAsync<List<CompanyDTO>>();

                return companies ?? new List<CompanyDTO>();
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP request failed during get companies");
                throw new Exception("Failed to fetch companies: " + ex.Message, ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during get companies");
                throw new Exception("Unexpected error fetching companies: " + ex.Message, ex);
            }
        }

        public async Task<CompanyDTO?> GetCompanyById(int id)
        {
            try
            {
                _logger.LogInformation("Starting to fetch company: {Id}", id);

                var httpClient = GetAuthenticatedClient("MainApi");
                var response = await httpClient.GetAsync($"Companies/get/{id}");

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Company not found: {Id}", id);
                    return null;
                }

                var company = await response.Content.ReadFromJsonAsync<CompanyDTO>();
                return company;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP request failed during get company by ID: {Id}", id);
                throw new Exception($"Failed to fetch company: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during get company by ID: {Id}", id);
                throw new Exception($"Unexpected error fetching company: {ex.Message}", ex);
            }
        }

        public async Task DeleteCompany(int id)
        {
            try
            {
                _logger.LogInformation("Starting to delete company: {Id}", id);

                var httpClient = GetAuthenticatedClient("MainApi");
                var response = await httpClient.DeleteAsync($"Companies/delete/{id}");
                response.EnsureSuccessStatusCode();

                _logger.LogInformation("Company deleted successfully: {Id}", id);
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP request exception during delete company: {Id}", id);
                throw new Exception($"Failed to delete company: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during delete company: {Id}", id);
                throw new Exception($"Unexpected error deleting company: {ex.Message}", ex);
            }
        }

        public async Task<ResponseResult> SaveUpdateCompany(CompanyDTO company)
        {
            try
            {
                _logger.LogInformation("Starting to save company");

                var httpClient = GetAuthenticatedClient("MainApi");
                var requestBody = new
                {
                    CompanyDTO = company
                };
                var content = JsonContent.Create(requestBody);
                var response = await httpClient.PostAsync("Companies/insert-update", content);
                response.EnsureSuccessStatusCode();

                var result = await response.Content.ReadFromJsonAsync<ResponseResult>();

                return result ?? new ResponseResult { IsSuccessStatus = false, Message = "No response from server" };
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP request failed during save company");
                throw new Exception("Failed to save company: " + ex.Message, ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during save company");
                throw new Exception("Unexpected error saving company: " + ex.Message, ex);
            }
        }
    }
}
