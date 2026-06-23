using JM.Infrastructure.Models;
using JM.UI.Entities.Model.Items;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace JM.UI.DataService.DAL.Items
{
    public interface IItemRepository
    {
        Task<IEnumerable<ItemDTO>> GetItems();
        Task<ItemDTO?> GetItemById(int id);
        Task<IEnumerable<ItemDTO>> LoadItemsBySubGroup(int subGroupId);
        //Task<ResponseResult> SaveUpdateItem(ItemDTO item);
        Task<ResponseResult> DeleteItem(int id);
        Task<IEnumerable<ItemDTO>> GetItemByPurchaseId(int purchaseId);
        Task<IEnumerable<ItemDTO>> GetItemsByStoreId(int storeId);
        Task<ResponseResult> CreateItem(CreateItemRequestDTO createItemRequest);
        Task<IEnumerable<TransactionTypeDTO>> GetTransactionTypes();
    }
}
