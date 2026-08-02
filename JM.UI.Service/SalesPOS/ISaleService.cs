using JM.Infrastructure.Models;
using JM.UI.Entities.Model.SalesPOS;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace JM.UI.Service.SalesPOS
{
    public interface ISaleService
    {
        Task<IEnumerable<SaleSummaryDTO>> GetAllSales();
        Task<IEnumerable<SaleSummaryDTO>> GetDraftSales();
        Task<IEnumerable<SaleSummaryDTO>> GetBookingSales();
        Task<SaleMasterDTO?> GetSaleById(int id);
        Task<ResponseResult> SaveSale(SaleMasterDTO sale);
        Task<ResponseResult> DeleteSale(int id);
        Task<IEnumerable<SaleSummaryDTO>> GetSalesByDateRange(DateTime fromDate, DateTime toDate);
        Task<IEnumerable<SaleSummaryDTO>> GetSalesByCustomerId(int customerId);
        Task<SaleMasterDTO?> GetSaleByInvoiceNo(string invoiceNo);
        Task<string> GetNewInvoiceNo();
        Task<ProductSearchDTO?> SearchByBarcode(string returnRefNo, int? storeId);
        Task<IEnumerable<ProductSearchDTO>> SearchProducts(string term);
        Task<(bool IsValid, string ErrorMessage)> ValidateSale(SaleMasterDTO sale);
        SaleMasterDTO CreateNew();
        decimal CalculateSubTotal(List<SaleDetailDTO> details);
        decimal CalculateVat(decimal subTotal, decimal vatPercentage);
        decimal CalculateNetAmount(SaleMasterDTO sale);
    }
}
