using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using JM.UI.DataService.DAL;
using JM.UI.Entities.Model.Dashboard;
using JM.UI.Entities.Services;
using Microsoft.Extensions.Logging;

namespace JM.UI.DataService.DAL.Dashboard
{
    public interface IDashboardRepository
    {
        Task<AdminDashboardDTO> GetAdminDashboardAsync(int? storeId, string period);
    }

    public class DashboardRepository : BaseRepository, IDashboardRepository
    {
        private readonly ILogger<DashboardRepository> _logger;

        public DashboardRepository(IHttpClientFactory httpClientFactory, ITokenProvider tokenProvider, ILogger<DashboardRepository> logger)
            : base(httpClientFactory, tokenProvider, logger)
        {
            _logger = logger;
        }

        private HttpClient Client => GetAuthenticatedClient();

        public async Task<AdminDashboardDTO> GetAdminDashboardAsync(int? storeId, string period)
        {
            var url = "Dashboard/admin?period=" + System.Uri.EscapeDataString(period ?? "Today");
            if (storeId.HasValue) url += $"&storeId={storeId.Value}";
            return await Client.GetFromJsonAsync<AdminDashboardDTO>(url) ?? new AdminDashboardDTO();
        }
    }
}
