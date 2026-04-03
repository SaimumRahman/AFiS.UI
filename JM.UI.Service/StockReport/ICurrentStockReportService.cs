using JM.UI.Entities.Model.StockReport_D;

namespace JM.UI.Service.StockReport
{
    public interface ICurrentStockReportService
    {
        string FormatCurrency(decimal amount);
        string FormatQty(decimal qty, string? uom);
        Task<CurrentStockReportResponseDTO> GetCurrentStockReport(CurrentStockReportFilterDTO filter);
    }
}