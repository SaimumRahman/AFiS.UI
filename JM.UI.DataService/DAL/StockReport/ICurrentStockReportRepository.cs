using JM.UI.Entities.Model.StockReport_D;

namespace JM.UI.DataService.DAL.StockReport
{
    public interface ICurrentStockReportRepository
    {
        Task<CurrentStockReportResponseDTO> GetCurrentStockReport(CurrentStockReportFilterDTO filter);
    }
}