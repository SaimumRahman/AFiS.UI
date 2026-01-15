using JM.Infrastructure.Models;
using JM.UI.Entities.Model.PurchaseOrders;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace JM.UI.DataService.DAL.PurchaseOrders
{
    public interface IPurchaseOrderRepository
    {
        Task<IEnumerable<PurchaseOrderModelDTO>> GetPurchaseOrders();
        Task<PurchaseOrderModelDTO?> GetPurchaseOrderById(int id);
        Task<ResponseResult> SaveUpdatePurchaseOrder(PurchaseOrderModelDTO purchaseOrder);
        Task DeletePurchaseOrder(int id);
    }
}
