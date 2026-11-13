using JM.Infrastructure.Models;
using JM.UI.Entities.Model.Sms;
using JM.UI.Entities.Services;
using Microsoft.Extensions.Logging;
using System;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace JM.UI.DataService.DAL.Sms
{
    public interface ISmsSettingsRepository
    {
        Task<SmsSettingsDTO> GetAsync();
        Task<ResponseResult> SaveAsync(SmsSettingsDTO dto, int modifiedBy);
        Task<SmsSendResultDTO> TestSendAsync(SmsSettingsDTO settings, string mobile, string message, int modifiedBy);
    }

    public class SmsSettingsRepository : BaseRepository, ISmsSettingsRepository
    {
        public SmsSettingsRepository(IHttpClientFactory httpClientFactory, ITokenProvider tokenProvider, ILogger<SmsSettingsRepository> logger)
            : base(httpClientFactory, tokenProvider, logger) { }

        public async Task<SmsSettingsDTO> GetAsync()
        {
            var http = GetAuthenticatedClient("MainApi");
            var res = await http.GetAsync("api/SmsSettings");
            res.EnsureSuccessStatusCode();
            return await res.Content.ReadFromJsonAsync<SmsSettingsDTO>() ?? new SmsSettingsDTO();
        }

        public async Task<ResponseResult> SaveAsync(SmsSettingsDTO dto, int modifiedBy)
        {
            var http = GetAuthenticatedClient("MainApi");
            var cmd = new { Settings = dto, ModifiedBy = modifiedBy };
            var res = await http.PutAsJsonAsync("api/SmsSettings", cmd);
            res.EnsureSuccessStatusCode();
            return await res.Content.ReadFromJsonAsync<ResponseResult>() ?? new ResponseResult { IsSuccessStatus = true };
        }

        public async Task<SmsSendResultDTO> TestSendAsync(SmsSettingsDTO settings, string mobile, string message, int modifiedBy)
        {
            var http = GetAuthenticatedClient("MainApi");
            var cmd = new { Request = new { Settings = settings, MobileNumber = mobile, TestMessage = message } };
            // stamp ModifiedBy into settings so server can enforce admin
            settings.ModifiedBy = modifiedBy;
            var res = await http.PostAsJsonAsync("api/SmsSettings/test-send", cmd);
            if (!res.IsSuccessStatusCode)
                return new SmsSendResultDTO { Success = false, ProviderMessage = $"HTTP {(int)res.StatusCode}" };
            return await res.Content.ReadFromJsonAsync<SmsSendResultDTO>() ?? new SmsSendResultDTO { Success = false };
        }
    }
}
