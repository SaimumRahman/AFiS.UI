using JM.Infrastructure.Models;
using JM.UI.DataService.DAL.UnitOfWork;
using JM.UI.Entities.Model.VoucherDetails;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace JM.UI.Service.VoucherDetails
{
    public class VoucherDetailsService : IVoucherDetailsService
    {
        private readonly IRepositoryUnitOfWork _unitOfWork;

        public VoucherDetailsService(IRepositoryUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IEnumerable<VoucherDetailsModelDTO>> GetVoucherDetails()
        {
            try
            {
                return await _unitOfWork.VoucherDetailsRepository.GetVoucherDetails();
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<VoucherDetailsModelDTO?> GetVoucherDetailsById(int id)
        {
            try
            {
                return await _unitOfWork.VoucherDetailsRepository.GetVoucherDetailsById(id);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<IEnumerable<VoucherDetailsModelDTO>> GetVoucherDetailsByVoucherId(int voucherId)
        {
            try
            {
                return await _unitOfWork.VoucherDetailsRepository.GetVoucherDetailsByVoucherId(voucherId);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<ResponseResult> SaveUpdateVoucherDetails(VoucherDetailsModelDTO voucherDetails)
        {
            try
            {
                return await _unitOfWork.VoucherDetailsRepository.SaveUpdateVoucherDetails(voucherDetails);
            }
            catch (Exception ex)
            {
                return new ResponseResult { IsSuccessStatus = false, Message = ex.Message };
            }
        }

        public async Task<ResponseResult> DeleteVoucherDetails(int id)
        {
            try
            {
                await _unitOfWork.VoucherDetailsRepository.DeleteVoucherDetails(id);
                return new ResponseResult { IsSuccessStatus = true, Message = "Voucher Details Deleted Successfully" };
            }
            catch (Exception ex)
            {
                return new ResponseResult { IsSuccessStatus = false, Message = ex.Message };
            }
        }
    }
}
