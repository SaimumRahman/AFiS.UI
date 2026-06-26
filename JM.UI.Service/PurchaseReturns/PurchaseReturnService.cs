using JM.Infrastructure.Models;
using JM.UI.DataService.DAL.UnitOfWork;
using JM.UI.Entities.Model.PurchaseReturns;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace JM.UI.Service.PurchaseReturns
{
    public class PurchaseReturnService : IPurchaseReturnService
    {
        private readonly IRepositoryUnitOfWork _unitOfWork;

        public PurchaseReturnService(IRepositoryUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IEnumerable<PurchaseReturnModelDTO>> GetPurchaseReturns()
        {
            try
            {
                return await _unitOfWork.PurchaseReturnRepository.GetPurchaseReturns();
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<PurchaseReturnModelDTO?> GetPurchaseReturnById(int id)
        {
            try
            {
                return await _unitOfWork.PurchaseReturnRepository.GetPurchaseReturnById(id);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<ResponseResult> SaveUpdatePurchaseReturn(PurchaseReturnModelDTO purchaseReturn)
        {
            try
            {
                return await _unitOfWork.PurchaseReturnRepository.SaveUpdatePurchaseReturn(purchaseReturn);
            }
            catch (Exception ex)
            {
                return new ResponseResult { IsSuccessStatus = false, Message = ex.Message };
            }
        }

        public async Task<ResponseResult> DeletePurchaseReturn(int id)
        {
            try
            {
                await _unitOfWork.PurchaseReturnRepository.DeletePurchaseReturn(id);
                return new ResponseResult { IsSuccessStatus = true, Message = "Purchase Return deleted successfully." };
            }
            catch (Exception ex)
            {
                return new ResponseResult { IsSuccessStatus = false, Message = ex.Message };
            }
        }

        public async Task<IEnumerable<ReturnRefStockDetailDTO>> GetReturnRefStockDetails(string returnRefNo, int storeId)
        {
            try
            {
                return await _unitOfWork.PurchaseReturnRepository.GetReturnRefStockDetails(returnRefNo, storeId);
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
