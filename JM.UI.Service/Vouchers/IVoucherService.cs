using JM.Infrastructure.Models;
using JM.UI.Entities.Model.Vouchers;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace JM.UI.Service.Vouchers
{
    public interface IVoucherService
    {
        Task<IEnumerable<VoucherModelDTO>> GetVouchers();
        Task<VoucherModelDTO?> GetVoucherById(int id);
        Task<ResponseResult> SaveUpdateVoucher(VoucherModelDTO voucher);
        Task<ResponseResult> DeleteVoucher(int id);
    }
}
