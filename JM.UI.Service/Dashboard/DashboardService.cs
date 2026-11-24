using System.Threading.Tasks;
using JM.UI.DataService.DAL.Dashboard;
using JM.UI.Entities.Model.Dashboard;

namespace JM.UI.Service.Dashboard
{
    public interface IDashboardService
    {
        Task<AdminDashboardDTO> GetAdminDashboardAsync(int? storeId, string period);
    }

    public class DashboardService : IDashboardService
    {
        private readonly IDashboardRepository _repo;
        public DashboardService(IDashboardRepository repo) => _repo = repo;

        public Task<AdminDashboardDTO> GetAdminDashboardAsync(int? storeId, string period)
            => _repo.GetAdminDashboardAsync(storeId, period);
    }
}
