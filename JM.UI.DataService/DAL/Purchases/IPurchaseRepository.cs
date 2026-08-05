using JM.Infrastructure.Models;
using JM.UI.Entities.Model.PurchaseItems;
using JM.UI.Entities.Model.Purchases;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace JM.UI.DataService.DAL.Purchases
{
    public interface IPurchaseRepository
    {
        Task<IEnumerable<PurchaseSummaryDTO>> GetPurchases(DateTime? fromDate, DateTime? toDate);
        Task<PurchaseDTO?> GetPurchaseById(int id);
        Task<ResponseResult> SaveUpdatePurchase(PurchaseDTO purchase, List<PurchaseItemDTO> items);
        Task DeletePurchase(int id);
        Task<string> GenerateBarcode(BarcodeGenerationRequestDTO request);
        Task<BarcodeSearchResponseDTO> SearchByBarcode(string barcode);
        Task<IEnumerable<PurchaseDraftDTO>> GetPurchaseDrafts();
        Task<PurchaseDraftDTO?> GetPurchaseDraftById(int id);
        Task<ResponseResult> SavePurchaseDraft(PurchaseDraftDTO draft, List<PurchaseDraftItemDTO> items);
        Task DeletePurchaseDraft(int id);
        Task<IEnumerable<PurchaseItemDTO>> GetPurchaseItems(int purchaseId);
        Task<SystemInVoiceDTO?> GetSystemInvoiceNew();
        Task<IEnumerable<PurchaseInvoiceDTO>> GetPurchasesByDateRange(DateTime fromDate, DateTime toDate);
    }
}
