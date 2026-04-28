using JM.Infrastructure.Models;
using JM.UI.Entities.Model.PurchaseItems;
using JM.UI.Entities.Model.Purchases;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace JM.UI.Service.Purchases
{
    public interface IPurchaseService
    {
        Task<IEnumerable<PurchaseSummaryDTO>> GetAllPurchases();
        Task<PurchaseDTO?> GetPurchaseById(int id);
        Task<ResponseResult> SaveUpdatePurchase(PurchaseDTO purchase, List<PurchaseItemDTO> items);
        Task<ResponseResult> DeletePurchase(int id);
        Task<string> GenerateBarcode(BarcodeGenerationRequestDTO request);
        Task<BarcodeSearchResponseDTO> SearchByBarcode(string barcode);
        Task<(bool IsValid, string ErrorMessage)> ValidatePurchase(PurchaseDTO purchase, List<PurchaseItemDTO> items);
        Task<PurchaseDTO> CreateNewPurchase();
        decimal CalculateItemTotal(PurchaseItemDTO item);
        decimal CalculatePurchaseTotal(List<PurchaseItemDTO> items);
        Task<IEnumerable<PurchaseDraftDTO>> GetPurchaseDrafts();
        Task<PurchaseDraftDTO?> GetPurchaseDraftById(int id);
        Task<ResponseResult> SavePurchaseDraft(PurchaseDraftDTO draft, List<PurchaseDraftItemDTO> items);
        Task<ResponseResult> DeletePurchaseDraft(int id);
        Task<(bool IsValid, string ErrorMessage)> ValidatePurchaseDraft(PurchaseDraftDTO draft, List<PurchaseDraftItemDTO> items);
        string FormatCurrency(decimal amount);
        string FormatDate(DateTime? date);
        Task<IEnumerable<PurchaseItemDTO>> GetPurchaseItems(int purchaseId);
        Task<SystemInVoiceDTO?> GetSystemInvoiceNew();
        Task<IEnumerable<PurchaseInvoiceDTO>> GetPurchasesByDateRange(DateTime fromDate, DateTime toDate);

    }
}
