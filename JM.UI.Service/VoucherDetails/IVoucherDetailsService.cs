using JM.Infrastructure.Models;
using JM.UI.Entities.Model.VoucherDetails;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace JM.UI.Service.VoucherDetails
{
    public interface IVoucherDetailsService
    {
        Task<IEnumerable<VoucherDetailsModelDTO>> GetVoucherDetails();
        Task<VoucherDetailsModelDTO?> GetVoucherDetailsById(int id);
        Task<IEnumerable<VoucherDetailsModelDTO>> GetVoucherDetailsByVoucherId(int voucherId);
        Task<ResponseResult> SaveUpdateVoucherDetails(VoucherDetailsModelDTO voucherDetails);
        Task<ResponseResult> DeleteVoucherDetails(int id);
    }
}
