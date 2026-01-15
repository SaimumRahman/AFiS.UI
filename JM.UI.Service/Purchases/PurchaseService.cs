using JM.Infrastructure.Models;
using JM.UI.DataService.DAL.UnitOfWork;
using JM.UI.Entities.Model.Purchases;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace JM.UI.Service.Purchases
{
    public class PurchaseService : IPurchaseService
    {
        private readonly IRepositoryUnitOfWork _unitOfWork;

        public PurchaseService(IRepositoryUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IEnumerable<PurchaseModelDTO>> GetPurchases()
        {
            try
            {
                return await _unitOfWork.PurchaseRepository.GetPurchases();
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<PurchaseModelDTO?> GetPurchaseById(int id)
        {
            try
            {
                return await _unitOfWork.PurchaseRepository.GetPurchaseById(id);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<ResponseResult> SaveUpdatePurchase(PurchaseModelDTO purchase)
        {
            try
            {
                return await _unitOfWork.PurchaseRepository.SaveUpdatePurchase(purchase);
            }
            catch (Exception ex)
            {
                return new ResponseResult { IsSuccessStatus = false, Message = ex.Message };
            }
        }

        public async Task<ResponseResult> DeletePurchase(int id)
        {
            try
            {
                await _unitOfWork.PurchaseRepository.DeletePurchase(id);
                return new ResponseResult { IsSuccessStatus = true, Message = "Purchase deleted successfully." };
            }
            catch (Exception ex)
            {
                return new ResponseResult { IsSuccessStatus = false, Message = ex.Message };
            }
        }
    }
}
