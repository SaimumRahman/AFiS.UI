using JM.Infrastructure.Models;
using JM.UI.Entities.Model.InvRequisition;
using JM.UI.Entities.Services;
using Microsoft.Extensions.Logging;
using System.Net.Http.Json;

namespace JM.UI.DataService.DAL.InvRequisition
{
    public class InvRequisitionRepository : BaseRepository, IInvRequisitionRepository
    {
        public InvRequisitionRepository(
            IHttpClientFactory httpClientFactory,
            ITokenProvider tokenProvider,
            ILogger<InvRequisitionRepository> logger)
            : base(httpClientFactory, tokenProvider, logger)
        {
        }

        public async Task<IEnumerable<InvRequisitionMasterDTO>> GetAll()
        {
            try
            {
                _logger.LogInformation("Fetching all requisitions");

                var httpClient = GetAuthenticatedClient("MainApi");
                var response = await httpClient.GetAsync("api/InvRequisition/getall");
                response.EnsureSuccessStatusCode();

                var result = await response.Content.ReadFromJsonAsync<List<InvRequisitionMasterDTO>>();
                return result ?? new List<InvRequisitionMasterDTO>();
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP request failed during get all requisitions");
                throw new Exception("Failed to fetch requisitions: " + ex.Message, ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during get all requisitions");
                throw new Exception("Unexpected error fetching requisitions: " + ex.Message, ex);
            }
        }

        public async Task<InvRequisitionMasterDTO?> GetById(int id)
        {
            try
            {
                _logger.LogInformation("Fetching requisition by id: {Id}", id);

                var httpClient = GetAuthenticatedClient("MainApi");
                var response = await httpClient.GetAsync($"api/InvRequisition/get/{id}");

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Requisition not found: {Id}", id);
                    return null;
                }

                return await response.Content.ReadFromJsonAsync<InvRequisitionMasterDTO>();
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP request failed during get requisition by id: {Id}", id);
                throw new Exception("Failed to fetch requisition: " + ex.Message, ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during get requisition by id: {Id}", id);
                throw new Exception("Unexpected error fetching requisition: " + ex.Message, ex);
            }
        }

        public async Task<ResponseResult> InsertUpdate(InvRequisitionMasterDTO requisition)
        {
            try
            {
                _logger.LogInformation("Saving requisition: {RequisitionID}", requisition.RequisitionID);

                var httpClient = GetAuthenticatedClient("MainApi");
                var command = new
                {
                    Requisition = requisition
                };
                var content = JsonContent.Create(command);
                
                var response = await httpClient.PostAsync("api/InvRequisition/insert-update", content);

                var rawBody = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("Requisition save failed. Status: {Status} | Body: {Body}",
                        (int)response.StatusCode, rawBody);

                    return new ResponseResult
                    {
                        IsSuccessStatus = false,
                        StatusCode = (int)response.StatusCode,
                        Message = $"Server error ({(int)response.StatusCode}): {rawBody}"
                    };
                }

                var result = System.Text.Json.JsonSerializer.Deserialize<ResponseResult>(rawBody,
                    new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                return result ?? new ResponseResult { IsSuccessStatus = false, Message = "No response from server." };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during save requisition");
                throw new Exception("Unexpected error saving requisition: " + ex.Message, ex);
            }
        }

        public async Task<ResponseResult> Delete(int id)
        {
            try
            {
                _logger.LogInformation("Deleting requisition: {Id}", id);

                var httpClient = GetAuthenticatedClient("MainApi");
                var response = await httpClient.DeleteAsync($"api/InvRequisition/delete/{id}");
                response.EnsureSuccessStatusCode();

                var result = await response.Content.ReadFromJsonAsync<ResponseResult>();
                return result ?? new ResponseResult { IsSuccessStatus = true, Message = "Requisition deleted successfully." };
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP request failed during delete requisition: {Id}", id);
                throw new Exception("Failed to delete requisition: " + ex.Message, ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during delete requisition: {Id}", id);
                throw new Exception("Unexpected error deleting requisition: " + ex.Message, ex);
            }
        }
    }
}
