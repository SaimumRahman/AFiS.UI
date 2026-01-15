using JM.Infrastructure.Models;
using JM.UI.Entities.Model.Purchases;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace JM.UI.DataService.DAL.Purchases
{
    public interface IPurchaseRepository
    {
        Task<IEnumerable<PurchaseModelDTO>> GetPurchases();
        Task<PurchaseModelDTO?> GetPurchaseById(int id);
        Task<ResponseResult> SaveUpdatePurchase(PurchaseModelDTO purchase);
        Task DeletePurchase(int id);
    }
}
