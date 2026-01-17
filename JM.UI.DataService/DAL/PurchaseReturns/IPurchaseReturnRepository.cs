using JM.Infrastructure.Models;
using JM.UI.Entities.Model.PurchaseReturns;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace JM.UI.DataService.DAL.PurchaseReturns
{
    public interface IPurchaseReturnRepository
    {
        Task<IEnumerable<PurchaseReturnModelDTO>> GetPurchaseReturns();
        Task<PurchaseReturnModelDTO?> GetPurchaseReturnById(int id);
        Task<ResponseResult> SaveUpdatePurchaseReturn(PurchaseReturnModelDTO purchaseReturn);
        Task DeletePurchaseReturn(int id);
    }
}
