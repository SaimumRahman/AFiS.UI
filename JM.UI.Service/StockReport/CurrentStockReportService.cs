using JM.UI.DataService.DAL.UnitOfWork;
using JM.UI.Entities.Model.StockReport_D;
using System;
using System.Collections.Generic;
using System.Text;

namespace JM.UI.Service.StockReport
{
    public class CurrentStockReportService : ICurrentStockReportService
    {
        private readonly IRepositoryUnitOfWork _repositoryUnitOfWork;

        public CurrentStockReportService(IRepositoryUnitOfWork repositoryUnitOfWork)
            => _repositoryUnitOfWork = repositoryUnitOfWork;

        public async Task<CurrentStockReportResponseDTO> GetCurrentStockReport(CurrentStockReportFilterDTO filter)
        {
            return await _repositoryUnitOfWork.CurrentStockReportRepository.GetCurrentStockReport(filter);
        }

        public string FormatQty(decimal qty, string? uom)
        {
            // Show decimals only if UoM is not Pcs
            return uom?.ToUpper() == "PCS"
                ? qty.ToString("N0")
                : qty.ToString("N2");
        }

        public string FormatCurrency(decimal amount)
        {
            return amount.ToString("N2");
        }
    }
}
