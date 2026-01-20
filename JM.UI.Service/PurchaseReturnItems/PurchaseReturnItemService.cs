using JM.Infrastructure.Models;
using JM.UI.DataService.DAL.UnitOfWork;
using JM.UI.Entities.Model.PurchaseReturnItems;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace JM.UI.Service.PurchaseReturnItems
{
    public class PurchaseReturnItemService : IPurchaseReturnItemService
    {
        private readonly IRepositoryUnitOfWork _unitOfWork;

        public PurchaseReturnItemService(IRepositoryUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IEnumerable<PurchaseReturnItemModelDTO>> GetAllReturnItems()
        {
            return await _unitOfWork.PurchaseReturnItemRepository.GetAllReturnItems();
        }

        public async Task<IEnumerable<PurchaseReturnItemModelDTO>> GetItemsByReturnId(int returnId)
        {
            return await _unitOfWork.PurchaseReturnItemRepository.GetItemsByReturnId(returnId);
        }

        public async Task<ResponseResult> SaveUpdateItem(PurchaseReturnItemModelDTO item)
        {
            return await _unitOfWork.PurchaseReturnItemRepository.SaveUpdateItem(item);
        }

        public async Task<ResponseResult> DeleteItem(int id)
        {
            try
            {
                await _unitOfWork.PurchaseReturnItemRepository.DeleteItem(id);
                return new ResponseResult { IsSuccessStatus = true, Message = "Item deleted successfully." };
            }
            catch (Exception ex)
            {
                return new ResponseResult { IsSuccessStatus = false, Message = ex.Message };
            }
        }
    }
}
