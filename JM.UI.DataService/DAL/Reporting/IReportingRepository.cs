using JM.UI.Entities.Model.Reporting_D;
using System;

namespace JM.UI.DataService.DAL.Reporting
{
    public interface IReportingRepository
    {
        Task<IEnumerable<ProfitLossReportDTO>> GetProfitLossReport(int? storeId, DateTime? fromDate, DateTime? toDate);
    }
}
