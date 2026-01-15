using JM.Infrastructure.Models;
using JM.UI.DataService.DAL.UnitOfWork;
using JM.UI.Entities.Model.PurchaseOrders;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace JM.UI.Service.PurchaseOrders
{
    public class PurchaseOrderService : IPurchaseOrderService
    {
        private readonly IRepositoryUnitOfWork _unitOfWork;

        public PurchaseOrderService(IRepositoryUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IEnumerable<PurchaseOrderModelDTO>> GetPurchaseOrders()
        {
            try
            {
                return await _unitOfWork.PurchaseOrderRepository.GetPurchaseOrders();
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<PurchaseOrderModelDTO?> GetPurchaseOrderById(int id)
        {
            try
            {
                return await _unitOfWork.PurchaseOrderRepository.GetPurchaseOrderById(id);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<ResponseResult> SaveUpdatePurchaseOrder(PurchaseOrderModelDTO purchaseOrder)
        {
            try
            {
                return await _unitOfWork.PurchaseOrderRepository.SaveUpdatePurchaseOrder(purchaseOrder);
            }
            catch (Exception ex)
            {
                return new ResponseResult { IsSuccessStatus = false, Message = ex.Message };
            }
        }

        public async Task<ResponseResult> DeletePurchaseOrder(int id)
        {
            try
            {
                await _unitOfWork.PurchaseOrderRepository.DeletePurchaseOrder(id);
                return new ResponseResult { IsSuccessStatus = true, Message = "Purchase Order deleted successfully." };
            }
            catch (Exception ex)
            {
                return new ResponseResult { IsSuccessStatus = false, Message = ex.Message };
            }
        }
    }
}
