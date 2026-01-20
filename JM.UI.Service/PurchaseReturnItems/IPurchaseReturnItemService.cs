using JM.Infrastructure.Models;
using JM.UI.Entities.Model.PurchaseReturnItems;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace JM.UI.Service.PurchaseReturnItems
{
    public interface IPurchaseReturnItemService
    {
        Task<IEnumerable<PurchaseReturnItemModelDTO>> GetAllReturnItems();
        Task<IEnumerable<PurchaseReturnItemModelDTO>> GetItemsByReturnId(int returnId);
        Task<ResponseResult> SaveUpdateItem(PurchaseReturnItemModelDTO item);
        Task<ResponseResult> DeleteItem(int id);
    }
}
