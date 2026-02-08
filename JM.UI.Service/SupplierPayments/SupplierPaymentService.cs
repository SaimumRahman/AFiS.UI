using JM.Infrastructure.Models;
using JM.UI.DataService.DAL.UnitOfWork;
using JM.UI.Entities.Model.SupplierPayments;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace JM.UI.Service.SupplierPayments
{
    public class SupplierPaymentService : ISupplierPaymentService
    {
        private readonly IRepositoryUnitOfWork _unitOfWork;

        public SupplierPaymentService(IRepositoryUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IEnumerable<SupplierPaymentDTO>> GetSupplierPayments()
        {
            try
            {
                return await _unitOfWork.SupplierPaymentRepository.GetSupplierPayments();
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<SupplierPaymentDTO?> GetSupplierPaymentById(int id)
        {
            try
            {
                return await _unitOfWork.SupplierPaymentRepository.GetSupplierPaymentById(id);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<ResponseResult> SaveUpdateSupplierPayment(SupplierPaymentDTO payment)
        {
            try
            {
                return await _unitOfWork.SupplierPaymentRepository.SaveUpdateSupplierPayment(payment);
            }
            catch (Exception ex)
            {
                return new ResponseResult { IsSuccessStatus = false, Message = ex.Message };
            }
        }

        public async Task<ResponseResult> DeleteSupplierPayment(int id)
        {
            try
            {
                await _unitOfWork.SupplierPaymentRepository.DeleteSupplierPayment(id);
                return new ResponseResult { IsSuccessStatus = true, Message = "Supplier Payment deleted successfully." };
            }
            catch (Exception ex)
            {
                return new ResponseResult { IsSuccessStatus = false, Message = ex.Message };
            }
        }

        public async Task<IEnumerable<SupplierLedgerDTO>> GetSupplierLedger(int supplierId)
        {
            try
            {
                return await _unitOfWork.SupplierPaymentRepository.GetSupplierLedger(supplierId);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<IEnumerable<SupplierOutstandingDTO>> GetSupplierOutstanding()
        {
            try
            {
                return await _unitOfWork.SupplierPaymentRepository.GetSupplierOutstanding();
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
