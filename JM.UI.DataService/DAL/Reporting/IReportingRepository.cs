using JM.UI.Entities.Model.Reporting_D;

namespace JM.UI.DataService.DAL.Reporting
{
    public interface IReportingRepository
    {
        Task<IEnumerable<ProfitLossReportDTO>> GetProfitLossReport(int? storeId);
    }
}
