using JM.Infrastructure.Models;
using JM.UI.Entities.Model.Vouchers;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace JM.UI.DataService.DAL.Vouchers
{
    public interface IVoucherRepository
    {
        Task<IEnumerable<VoucherModelDTO>> GetVouchers();
        Task<VoucherModelDTO?> GetVoucherById(int id);
        Task<ResponseResult> SaveUpdateVoucher(VoucherModelDTO voucher);
        Task DeleteVoucher(int id);
    }
}
