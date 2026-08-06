using JM.Infrastructure.Models;
using JM.UI.Entities.Model.SalesPOS;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace JM.UI.DataService.DAL.SalesPOS
{
    public interface ISaleRepository
    {
        Task<IEnumerable<SaleSummaryDTO>> GetSales();
        Task<IEnumerable<SaleSummaryDTO>> GetDraftSales();
        Task<IEnumerable<SaleSummaryDTO>> GetBookingSales();
        Task<SaleMasterDTO?> GetSaleById(int id);
        Task<ResponseResult> SaveSale(SaleMasterDTO sale);
        Task<ResponseResult> SaveDuePayment(int saleMasterId, int storeId, List<PaymentTransactionDTO> payments, int createdBy);
        Task<ResponseResult> UnmarkDraftSale(int saleMasterId);
        Task<ResponseResult> DeleteSale(int id);
        Task<IEnumerable<SaleSummaryDTO>> GetSalesByDateRange(DateTime fromDate, DateTime toDate);
        Task<IEnumerable<SaleSummaryDTO>> GetSalesByCustomerId(int customerId);
        Task<SaleMasterDTO?> GetSaleByInvoiceNo(string invoiceNo);
        Task<string> GetNewInvoiceNo();
        Task<ProductSearchDTO?> SearchByBarcode(string returnRefNo, int storeId);
        Task<IEnumerable<ProductSearchDTO>> SearchProducts(string term);
    }
}
