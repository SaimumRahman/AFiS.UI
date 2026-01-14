using JM.Infrastructure.Models;
using JM.UI.Entities.Model.VoucherDetails;
using JM.UI.Entities.Services;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace JM.UI.DataService.DAL.VoucherDetails
{
    public class VoucherDetailsRepository : BaseRepository, IVoucherDetailsRepository
    {
        public VoucherDetailsRepository(
            IHttpClientFactory httpClientFactory,
            ITokenProvider tokenProvider,
            ILogger<VoucherDetailsRepository> logger)
            : base(httpClientFactory, tokenProvider, logger)
        {
        }

        public async Task<IEnumerable<VoucherDetailsModelDTO>> GetVoucherDetails()
        {
            try
            {
                var httpClient = GetAuthenticatedClient("MainApi");
                var response = await httpClient.GetAsync("VoucherDetails/GetAllVoucherDetails");
                response.EnsureSuccessStatusCode();

                var details = await response.Content.ReadFromJsonAsync<List<VoucherDetailsModelDTO>>();
                return details ?? new List<VoucherDetailsModelDTO>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching all voucher details");
                throw;
            }
        }

        public async Task<VoucherDetailsModelDTO?> GetVoucherDetailsById(int id)
        {
            try
            {
                var httpClient = GetAuthenticatedClient("MainApi");
                var response = await httpClient.GetAsync($"VoucherDetails/GetVoucherDetailsById/{id}");

                if (!response.IsSuccessStatusCode)
                    return null;

                return await response.Content.ReadFromJsonAsync<VoucherDetailsModelDTO>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching voucher details by ID: {Id}", id);
                throw;
            }
        }

        public async Task<IEnumerable<VoucherDetailsModelDTO>> GetVoucherDetailsByVoucherId(int voucherId)
        {
            try
            {
                var httpClient = GetAuthenticatedClient("MainApi");
                var response = await httpClient.GetAsync($"VoucherDetails/GetVoucherDetailsByVoucherId/{voucherId}");
                response.EnsureSuccessStatusCode();

                var details = await response.Content.ReadFromJsonAsync<List<VoucherDetailsModelDTO>>();
                return details ?? new List<VoucherDetailsModelDTO>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching voucher details for VoucherID: {VoucherId}", voucherId);
                throw;
            }
        }

        public async Task<ResponseResult> SaveUpdateVoucherDetails(VoucherDetailsModelDTO voucherDetails)
        {
            try
            {
                var httpClient = GetAuthenticatedClient("MainApi");
                var requestBody = new { VoucherDetailsDTO = voucherDetails };
                var response = await httpClient.PostAsJsonAsync("VoucherDetails/InsertUpdateVoucherDetails", requestBody);
                response.EnsureSuccessStatusCode();

                var result = await response.Content.ReadFromJsonAsync<ResponseResult>();
                return result ?? new ResponseResult { IsSuccessStatus = false, Message = "No response from server" };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving voucher details");
                throw;
            }
        }

        public async Task DeleteVoucherDetails(int id)
        {
            try
            {
                var httpClient = GetAuthenticatedClient("MainApi");
                var response = await httpClient.DeleteAsync($"VoucherDetails/DeleteVoucherDetails/{id}");
                response.EnsureSuccessStatusCode();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting voucher details: {Id}", id);
                throw;
            }
        }
    }
}
