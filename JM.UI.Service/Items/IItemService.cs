using JM.Infrastructure.Models;
using JM.UI.Entities.Model.Items;
using JM.UI.Entities.Model.PurchaseItems;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace JM.UI.Service.Items
{
    public interface IItemService
    {
        Task<IEnumerable<ItemDTO>> GetItems();
        Task<ItemDTO?> GetItemById(int id);
        Task<IEnumerable<ItemDTO>> LoadItemsBySubGroup(int subGroupId);
        Task<ResponseResult> DeleteItem(int id);
        Task<IEnumerable<ItemDTO>> GetItemsByStoreId(int storeId);
        Task<ResponseResult> CreateItem(PreviewItemRow createItemRequest);
        Task<ResponseResult> UpdateItem(UpdateItemDTO item);
        Task<IEnumerable<ItemDTO>> GetItemByPurchaseId(int purchaseId);
        Task<IEnumerable<TransactionTypeDTO>> GetTransactionTypes();
        Task<IEnumerable<PurchaseItemDTO>> GetItemByGroupIdWithStock(int groupId);
    }
}
