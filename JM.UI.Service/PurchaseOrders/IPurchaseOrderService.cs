using JM.Infrastructure.Models;
using JM.UI.Entities.Model.PurchaseOrders;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace JM.UI.Service.PurchaseOrders
{
    public interface IPurchaseOrderService
    {
        Task<IEnumerable<PurchaseOrderModelDTO>> GetPurchaseOrders();
        Task<PurchaseOrderModelDTO?> GetPurchaseOrderById(int id);
        Task<ResponseResult> SaveUpdatePurchaseOrder(PurchaseOrderModelDTO purchaseOrder);
        Task<ResponseResult> DeletePurchaseOrder(int id);
    }
}
