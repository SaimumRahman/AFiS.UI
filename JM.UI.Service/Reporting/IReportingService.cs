using JM.UI.Entities.Model.Reporting_D;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace JM.UI.Service.Reporting
{
    public interface IReportingService
    {
        Task<IEnumerable<ProfitLossReportDTO>> GetProfitLossReport(int? storeId);
    }
}
