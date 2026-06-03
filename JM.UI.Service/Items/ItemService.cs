using JM.Infrastructure.Models;
using JM.UI.DataService.DAL.UnitOfWork;
using JM.UI.Entities.Model.Items;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace JM.UI.Service.Items;

public class ItemService : IItemService
{
    private readonly IRepositoryUnitOfWork _unitOfWork;

    public ItemService(IRepositoryUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<IEnumerable<ItemDTO>> GetItems()
    {
        return await _unitOfWork.ItemRepository.GetItems();
    }
    public async Task<IEnumerable<ItemDTO>> GetItemsByStoreId(int storeId)
    {
        return await _unitOfWork.ItemRepository.GetItemsByStoreId(storeId);
    }

    public async Task<ItemDTO?> GetItemById(int id)
    {
        return await _unitOfWork.ItemRepository.GetItemById(id);
    }

    public async Task<ResponseResult> SaveUpdateItem(ItemDTO item)
    {
        return await _unitOfWork.ItemRepository.SaveUpdateItem(item);
    }

    public async Task<ResponseResult> DeleteItem(int id)
    {
        try
        {
            await _unitOfWork.ItemRepository.DeleteItem(id);
            return new ResponseResult { IsSuccessStatus = true, Message = "Item deleted successfully." };
        }
        catch (Exception ex)
        {
            return new ResponseResult { IsSuccessStatus = false, Message = ex.Message };
        }
    }

    public async Task<IEnumerable<ItemDTO>> LoadItemsBySubGroup(int subGroupId)
    {
        try
        {
            return await _unitOfWork.ItemRepository.LoadItemsBySubGroup(subGroupId);
        }
        catch (Exception)
        {
            throw;
        }
    }
}
