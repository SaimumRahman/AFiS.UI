using JM.Infrastructure.Models;
using JM.UI.Entities.Model.Purchases;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace JM.UI.Service.Purchases
{
    public interface IPurchaseService
    {
        Task<IEnumerable<PurchaseModelDTO>> GetPurchases();
        Task<PurchaseModelDTO?> GetPurchaseById(int id);
        Task<ResponseResult> SaveUpdatePurchase(PurchaseModelDTO purchase);
        Task<ResponseResult> DeletePurchase(int id);
    }
}
