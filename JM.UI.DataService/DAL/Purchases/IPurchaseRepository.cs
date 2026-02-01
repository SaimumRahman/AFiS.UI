using JM.Infrastructure.Models;
using JM.UI.Entities.Model.PurchaseItems;
using JM.UI.Entities.Model.Purchases;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace JM.UI.DataService.DAL.Purchases
{
    public interface IPurchaseRepository
    {
        Task<IEnumerable<PurchaseSummaryDTO>> GetPurchases();
        Task<PurchaseDTO?> GetPurchaseById(int id);
        Task<ResponseResult> SaveUpdatePurchase(PurchaseDTO purchase, List<PurchaseItemDTO> items);
        Task DeletePurchase(int id);
        Task<string> GenerateBarcode(BarcodeGenerationRequestDTO request);
        Task<BarcodeSearchResponseDTO> SearchByBarcode(string barcode);

    }
}
