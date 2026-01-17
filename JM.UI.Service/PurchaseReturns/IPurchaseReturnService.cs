using JM.Infrastructure.Models;
using JM.UI.Entities.Model.PurchaseReturns;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace JM.UI.Service.PurchaseReturns
{
    public interface IPurchaseReturnService
    {
        Task<IEnumerable<PurchaseReturnModelDTO>> GetPurchaseReturns();
        Task<PurchaseReturnModelDTO?> GetPurchaseReturnById(int id);
        Task<ResponseResult> SaveUpdatePurchaseReturn(PurchaseReturnModelDTO purchaseReturn);
        Task<ResponseResult> DeletePurchaseReturn(int id);
    }
}
