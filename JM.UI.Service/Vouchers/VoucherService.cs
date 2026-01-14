using JM.Infrastructure.Models;
using JM.UI.DataService.DAL.UnitOfWork;
using JM.UI.Entities.Model.Vouchers;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace JM.UI.Service.Vouchers
{
    public class VoucherService : IVoucherService
    {
        private readonly IRepositoryUnitOfWork _unitOfWork;

        public VoucherService(IRepositoryUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IEnumerable<VoucherModelDTO>> GetVouchers()
        {
            try
            {
                return await _unitOfWork.VoucherRepository.GetVouchers();
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<VoucherModelDTO?> GetVoucherById(int id)
        {
            try
            {
                return await _unitOfWork.VoucherRepository.GetVoucherById(id);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<ResponseResult> SaveUpdateVoucher(VoucherModelDTO voucher)
        {
            try
            {
                return await _unitOfWork.VoucherRepository.SaveUpdateVoucher(voucher);
            }
            catch (Exception ex)
            {
                return new ResponseResult { IsSuccessStatus = false, Message = ex.Message };
            }
        }

        public async Task<ResponseResult> DeleteVoucher(int id)
        {
            try
            {
                await _unitOfWork.VoucherRepository.DeleteVoucher(id);
                return new ResponseResult { IsSuccessStatus = true, Message = "Voucher Deleted Successfully" };
            }
            catch (Exception ex)
            {
                return new ResponseResult { IsSuccessStatus = false, Message = ex.Message };
            }
        }
    }
}
