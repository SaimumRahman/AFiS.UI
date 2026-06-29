using JM.Infrastructure.Models;
using JM.UI.Entities.Model.Coupon;

namespace JM.UI.DataService.DAL.Coupon
{
    public interface ICouponTypeRepository
    {
        Task<IEnumerable<CouponTypeDTO>> GetAll();
        Task<CouponTypeDTO?> GetById(int id);
        Task<ResponseResult> SaveUpdate(CouponTypeDTO couponType);
        Task<ResponseResult> Delete(int id);
    }
}
