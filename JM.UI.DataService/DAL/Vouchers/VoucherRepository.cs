using JM.Infrastructure.Models;
using JM.UI.Entities.Model.Vouchers;
using JM.UI.Entities.Services;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace JM.UI.DataService.DAL.Vouchers
{
    public class VoucherRepository : BaseRepository, IVoucherRepository
    {
        public VoucherRepository(
            IHttpClientFactory httpClientFactory,
            ITokenProvider tokenProvider,
            ILogger<VoucherRepository> logger)
            : base(httpClientFactory, tokenProvider, logger)
        {
        }

        public async Task<IEnumerable<VoucherModelDTO>> GetVouchers()
        {
            try
            {
                var httpClient = GetAuthenticatedClient("MainApi");
                var response = await httpClient.GetAsync("Vouchers/GetAllVouchers");
                response.EnsureSuccessStatusCode();

                var vouchers = await response.Content.ReadFromJsonAsync<List<VoucherModelDTO>>();
                return vouchers ?? new List<VoucherModelDTO>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching all vouchers");
                throw;
            }
        }

        public async Task<VoucherModelDTO?> GetVoucherById(int id)
        {
            try
            {
                var httpClient = GetAuthenticatedClient("MainApi");
                var response = await httpClient.GetAsync($"Vouchers/GetVoucherById/{id}");

                if (!response.IsSuccessStatusCode)
                    return null;

                return await response.Content.ReadFromJsonAsync<VoucherModelDTO>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching voucher by ID: {Id}", id);
                throw;
            }
        }

        public async Task<ResponseResult> SaveUpdateVoucher(VoucherModelDTO voucher)
        {
            try
            {
                var httpClient = GetAuthenticatedClient("MainApi");
                var requestBody = new { VoucherDTO = voucher };
                var response = await httpClient.PostAsJsonAsync("Vouchers/InsertUpdateVoucher", requestBody);
                response.EnsureSuccessStatusCode();

                var result = await response.Content.ReadFromJsonAsync<ResponseResult>();
                return result ?? new ResponseResult { IsSuccessStatus = false, Message = "No response from server" };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving voucher");
                throw;
            }
        }

        public async Task DeleteVoucher(int id)
        {
            try
            {
                var httpClient = GetAuthenticatedClient("MainApi");
                var response = await httpClient.DeleteAsync($"Vouchers/DeleteVoucher/{id}");
                response.EnsureSuccessStatusCode();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting voucher: {Id}", id);
                throw;
            }
        }
    }
}
